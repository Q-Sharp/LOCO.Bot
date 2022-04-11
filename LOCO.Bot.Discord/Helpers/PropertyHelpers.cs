using LOCO.Bot.Shared;

using System.Reflection;
using System.Text.Json.Serialization;

namespace LOCO.Bot.Discord.Helpers;

public static class PropertyHelpers
{
    private static readonly BindingFlags _cisBF 
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase;

    public static void SetProperty<T>(this T m, string propertyName, string newValue)
        where T : class
    {
        try
        {
            var pr = m.GetType().GetProperty(propertyName, _cisBF);
            if (pr is not null)
            {
                var t = Nullable.GetUnderlyingType(pr.PropertyType) ?? pr.PropertyType;
                object safeValue = null;

                if (t is not null)
                    if (TimeSpan.TryParse(newValue.ToString(), out var tSpan))
                        safeValue = tSpan;
                    else
                        safeValue = Convert.ChangeType(newValue, t);

                var val = Convert.ChangeType(safeValue, t);
                var oldPropValue = m.GetType().GetProperty(propertyName, _cisBF)?.GetValue(m, null);
                m.GetType().GetProperty(propertyName, _cisBF)?.SetValue(m, val);
                var newPropValue = m.GetType().GetProperty(propertyName, _cisBF)?.GetValue(m, null);
            }
        }
        catch
        {
        }
    }

    public static void SetProperties<T>(this T m, T updateWith)
        where T : class
    {
        try
        {
            foreach (var p in m.GetType().GetProperties().Where(x => x.Name != nameof(IHaveId.Id)
                 && !x.CustomAttributes.Any(x => x.AttributeType == typeof(JsonIgnoreAttribute))))
                m.SetProperty(p.Name, updateWith.GetProperty(p.Name) as string);
        }
        catch
        {
        }
    }

    public static object GetProperty<T>(this T m, string propertyName)
        where T : class
    {
        try
        {
            var pr = m.GetType().GetProperty(propertyName, _cisBF);
            if (pr is not null)
            {
                var t = Nullable.GetUnderlyingType(pr.PropertyType) ?? pr.PropertyType;
                return m.GetType().GetProperty(propertyName, _cisBF)?.GetValue(m, null);
            }
        }
        catch
        {
        }

        return default;
    }

    public static object GetPropertyValue(this object src, string propName)
    {
        try
        {
            if (src is null || propName is null)
                return default;

            if (propName.Contains("."))
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return src.GetPropertyValue(temp[0]).GetPropertyValue(temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop?.GetValue(src, null);
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}
