using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ViewModels;

namespace BusinessLogic.ExtensionMethods
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySearch<T>(
    this IQueryable<T> query,
    string? search,
    params Expression<Func<T, string?>>[] properties)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim().ToLower();

            var parameter = Expression.Parameter(typeof(T), "x");

            Expression? body = null;

            foreach (var property in properties)
            {
                Expression member = ReplaceParameter(
                    property.Body,
                    property.Parameters[0],
                    parameter);

                member = Expression.Coalesce(
                    member,
                    Expression.Constant(string.Empty));

                member = Expression.Call(
                    member,
                    typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);

                var contains = Expression.Call(
                    member,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                    Expression.Constant(search));

                body = body == null
                    ? contains
                    : Expression.OrElse(body, contains);
            }

            return query.Where(
                Expression.Lambda<Func<T, bool>>(body!, parameter));
        }
        public static IQueryable<T> ApplySorting<T>(
    this IQueryable<T> query,
    string? sortColumn,
    string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return query;

            var property = typeof(T).GetProperties()
                .FirstOrDefault(p =>
                    p.Name.Equals(sortColumn,
                        StringComparison.OrdinalIgnoreCase));

            if (property == null)
                return query;

            return sortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : query.OrderBy(e => EF.Property<object>(e, property.Name));
        }

        public static IQueryable<T> ApplyPagination<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);
        }

        public static IQueryable<T> ApplyFilters<T>(
    this IQueryable<T> query,
    Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0)
                return query;

            foreach (var filter in filters)
            {
                var property = typeof(T).GetProperty(
                    filter.Key,
                    BindingFlags.IgnoreCase |
                    BindingFlags.Public |
                    BindingFlags.Instance);

                if (property == null)
                    continue;

                var parameter = Expression.Parameter(typeof(T), "x");

                var member = Expression.Property(parameter, property);

                Expression body;

                if (property.PropertyType == typeof(string))
                {
                    body = Expression.Call(
                        member,
                        nameof(string.Contains),
                        Type.EmptyTypes,
                        Expression.Constant(filter.Value));
                }
                else
                {
                    body = Expression.Equal(
                        member,
                        Expression.Constant(
                            Convert.ChangeType(
                                filter.Value,
                                property.PropertyType)));
                }

                var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

                query = query.Where(lambda);
            }

            return query;
        }
        private static Expression ReplaceParameter(
   Expression expression,
   ParameterExpression source,
   ParameterExpression target)
        {
            return new ParameterReplacer(source, target)
                .Visit(expression)!;
        }
        private sealed class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ParameterReplacer(
                ParameterExpression source,
                ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source
                    ? _target
                    : base.VisitParameter(node);
            }
        }
    }
}
