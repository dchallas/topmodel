﻿using TopModel.Core;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa;

public static class ImportsJpaExtensions
{

    public static List<string> GetTypeImports(this IProperty p, JpaConfig config, string tag)
    {
        return p switch
        {
            CompositionProperty cp => cp.GetTypeImports(config, tag),
            AssociationProperty ap => ap.GetTypeImports(config, tag),
            IFieldProperty fp => fp.GetTypeImports(config, tag),
            _ => new List<string>(),
        };
    }

    public static List<string> GetTypeImports(this IFieldProperty rp, JpaConfig config, string tag)
    {
        var imports = new List<string>();

        var javaType = config.GetImplementation(rp.Domain);
        if (javaType?.Imports != null)
        {
            imports.AddRange(javaType.Imports.Select(i => i.ParseTemplate(rp)));
        }

        if (rp is AliasProperty apo)
        {
            imports.AddRange(apo.GetTypeImports(config, tag));
        }
        else if (rp is RegularProperty rpr)
        {
            imports.AddRange(rpr.GetTypeImports(config, tag));
        }

        return imports;
    }

    public static List<string> GetTypeImports(this AliasProperty ap, JpaConfig config, string tag)
    {
        var imports = new List<string>();
        if (ap.Class != null && ap.Class.IsPersistent && ap.Property is AssociationProperty asp)
        {
            if (asp.IsEnum())
            {
                imports.AddRange(asp.Property.GetTypeImports(config, tag));
            }
        }

        if (ap.IsEnum())
        {
            imports.Add(ap.Property.Class.GetImport(config, tag));
        }
        else if (ap.Property is AssociationProperty apr && ap.IsAssociatedEnum())
        {
            imports.Add(apr.Association.GetImport(config, tag));
        }

        if (ap.Property is AssociationProperty apo && (apo.Type == AssociationType.ManyToMany || apo.Type == AssociationType.OneToMany))
        {
            imports.Add("java.util.List");
        }

        return imports;
    }

    public static List<string> GetTypeImports(this RegularProperty rp, JpaConfig config, string tag)
    {
        var imports = new List<string>();
        if (rp.IsEnum())
        {
            imports.Add($"{rp.Class.GetImport(config, tag)}");
        }

        imports.AddRange(config.GetImplementation(rp.Domain)!.Imports.Select(i => i.ParseTemplate(rp)));

        return imports;
    }

    public static List<string> GetTypeImports(this AssociationProperty ap, JpaConfig config, string tag)
    {
        var persistenceRoot = $"{config.PersistenceMode.ToString().ToLower()}.persistence.";
        var imports = new List<string>();

        switch (ap.Type)
        {
            case AssociationType.OneToMany:
            case AssociationType.ManyToMany:
                imports.Add("java.util.List");
                break;
        }

        if (ap.Association.Namespace.Module != ap.Class.Namespace.Module)
        {
            imports.Add(ap.Association.GetImport(config, tag));
        }

        if (ap.Association.Reference)
        {
            imports.AddRange(ap.Property.GetTypeImports(config, tag));
        }

        return imports;
    }

    public static List<string> GetTypeImports(this CompositionProperty cp, JpaConfig config, string tag)
    {
        var imports = new List<string>();
        if (cp.Composition.Namespace.Module != cp.Class?.Namespace.Module)
        {
            imports.Add(cp.Composition.GetImport(config, tag));
        }

        if (cp.Kind == "list")
        {
            imports.Add("java.util.List");
        }
        else if (cp.DomainKind != null)
        {
            imports.AddRange(config.GetImplementation(cp.DomainKind)!.Imports.Select(i => i.ParseTemplate(cp)));
        }

        return imports;
    }

    public static List<string> GetKindImports(this CompositionProperty cp, JpaConfig config)
    {
        var imports = new List<string>();

        if (cp.Kind == "list")
        {
            imports.Add("java.util.List");
        }
        else if (cp.DomainKind != null)
        {
            imports.AddRange(config.GetImplementation(cp.DomainKind)!.Imports.Select(i => i.ParseTemplate(cp)));
        }

        return imports;
    }

    public static string GetImport(this Class classe, JpaConfig config, string tag)
    {
        return $"{config.GetPackageName(classe, tag)}.{classe.NamePascal}";
    }

    public static List<string> GetImports(this Class classe, List<Class> classes, JpaConfig config, string tag)
    {
        var javaOrJakarta = config.PersistenceMode.ToString().ToLower();
        var imports = new List<string>();

        if (classe.Extends != null)
        {
            imports.Add(classe.GetImport(config, tag));
        }

        imports
        .AddRange(
            classe.FromMappers.SelectMany(fm => fm.Params).Select(fmp => fmp.Class.GetImport(config, tag)));
        imports
        .AddRange(
            classe.ToMappers.Select(fmp => fmp.Class.GetImport(config, tag)));

        // Suppression des imports inutiles
        return imports;
    }
}
