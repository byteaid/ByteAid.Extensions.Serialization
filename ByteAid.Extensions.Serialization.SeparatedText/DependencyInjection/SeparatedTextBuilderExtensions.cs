using ByteAid.Extensions.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ByteAid.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class SeparatedTextBuilderExtensions
{
    public static SerializationBuilder ConfigureSeparatedText<T>(
        this SerializationBuilder builder,
        Action<SeparatedTextRuleBuilder<T>> configureRules,
        ValueSeparator separatorType)
    {
        var ruleBuilder = new SeparatedTextRuleBuilder<T>();

        // Añadir regla de separador primero
        ruleBuilder.AddRule(new SeparatorSerializationRule
        {
            SeparatorType = separatorType
        });

        configureRules(ruleBuilder);

        var ruleset = new SerializationRuleset
        {
            Target = typeof(T),
            Rules = ruleBuilder.Build()
        };

        builder.Services.AddSingleton<ISerializationRuleset>(ruleset);

        return builder;
    }

    public static SeparatedTextRuleBuilder<T> HasHeaders<T>(this SeparatedTextRuleBuilder<T> builder, bool hasHeaders = true)
    {
        builder.AddRule(new HeaderSerializationRule
        {
            HasHeaders = hasHeaders
        });

        return builder;
    }

    public static SeparatedTextRuleBuilder<T> Property<T>(
        this SeparatedTextRuleBuilder<T> builder,
        Expression<Func<T, object?>> property,
        int position,
        string? headerName = null,
        bool required = false)
    {
        MemberExpression? member = property.Body as MemberExpression;

        if (member == null)
        {
            if (property.Body is UnaryExpression unary &&
                unary.NodeType == ExpressionType.Convert &&
                unary.Operand is MemberExpression)
            {
                member = unary.Operand as MemberExpression;
            }
        }

        if (member == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        builder.AddRule(new PropertySerializationRule
        {
            Property = member.Member.Name,
            Position = position,
            IsRequired = required,
            HeaderName = headerName
        });

        return builder;
    }
}