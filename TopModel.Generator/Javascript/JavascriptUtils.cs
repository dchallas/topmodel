﻿using TopModel.Core;

namespace TopModel.Generator.Javascript;

public static class JavascriptUtils
{
    public static string GetPropertyTypeName(this IProperty property, IEnumerable<Class>? availableClasses = null)
    {
        if (property is CompositionProperty cp)
        {
            return cp.Kind switch
            {
                "object" => cp.Composition.Name,
                "list" or "async-list" => $"{cp.Composition.Name}[]",
                string _ when cp.DomainKind!.TS!.Type.Contains("{composition.name}") => cp.DomainKind.TS.Type.ParseTemplate(cp),
                string _ => $"{cp.DomainKind.TS.Type}<{{composition.name}}>".ParseTemplate(cp)
            };
        }

        var fp = (IFieldProperty)property;

        if (fp.Domain.TS == null)
        {
            throw new ModelException(fp.Domain, $"Le type Typescript du domaine doit être renseigné.");
        }

        var fixedType = fp.Domain.TS.Type.ParseTemplate(fp);

        var prop = fp is AliasProperty alp ? alp.Property : fp;

        if (prop is AssociationProperty ap && ap.Association.Reference && !ap.Property.Domain.AutoGeneratedValue && (availableClasses == null || availableClasses.Contains(ap.Association)))
        {
            fixedType = $"{ap.Association.Name}{ap.Property.Name}";

            if (fp is AliasProperty { AsList: true })
            {
                fixedType += "[]";
            }
        }
        else if (prop is RegularProperty { ReferenceKey: true } && (availableClasses == null || availableClasses.Contains(prop.Class)))
        {
            fixedType = $"{prop.Class.Name}{prop.Name}";

            if (fp is AliasProperty { AsList: true })
            {
                fixedType += "[]";
            }
        }

        if (fp is AliasProperty { Property: AssociationProperty { Type: AssociationType.ManyToMany or AssociationType.OneToMany } })
        {
            fixedType += "[]";
        }

        return fixedType;
    }

    public static List<(string Import, string Path)> GroupAndSort(this IEnumerable<(string Import, string Path)> imports)
    {
        return imports
             .GroupBy(i => i.Path)
             .Select(i => (Import: string.Join(", ", i.Select(l => l.Import).Distinct().OrderBy(x => x)), Path: i.Key))
             .OrderBy(i => i.Path.StartsWith(".") ? i.Path : $"...{i.Path}")
             .ToList();
    }
}
