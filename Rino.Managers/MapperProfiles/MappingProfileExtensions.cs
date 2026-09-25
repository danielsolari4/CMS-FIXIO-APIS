using System;
using System.Linq;
using AutoMapper;

namespace Rino.Managers.MapperProfiles
{
    internal static class MappingProfileExtensions
    {
        private static readonly DateTime SqlDateTimeMinValue = new DateTime(1753, 1, 1);
        public static IMappingExpression<TSource, TDestination> PreserveLegacyUpdateBehavior<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map,
            params string[] mappedMembers)
        {
            var destinationType = typeof(TDestination);
            var mapped = mappedMembers.ToHashSet(StringComparer.Ordinal);

            foreach (var property in destinationType.GetProperties().Where(p => !mapped.Contains(p.Name)))
            {
                map.ForMember(property.Name, opt => opt.Ignore());
            }

            map.ForAllMembers(opt => opt.Condition((src, dest, sourceMember) =>
            {
                if (sourceMember is DateTime date)
                    return date >= SqlDateTimeMinValue;

                return true;
            }));

            return map;
        }
    }
}
