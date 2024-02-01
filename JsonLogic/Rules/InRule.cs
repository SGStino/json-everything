﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `in` operation.
/// </summary>
[Operator("in")]
[JsonConverter(typeof(InRuleJsonConverter))]
public class InRule : Rule
{
	/// <summary>
	/// A value to search for.
	/// </summary>
	protected internal Rule Test { get; }
	/// <summary>
	/// The string value to test against.
	/// </summary>
	protected internal Rule Value { get; }

	/// <summary>
	/// Creates a new instance of <see cref="InRule"/> when 'in' operator is detected within json logic.
	/// </summary>
	/// <param name="test">A value to search for.</param>
	/// <param name="value">The string value to test against.</param>
	protected internal InRule(Rule test, Rule value)
	{
		Test = test;
		Value = value;
	}

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var test = Test.Apply(data, contextData);
		var source = Value.Apply(data, contextData);

		if (source is JsonValue value && value.TryGetValue(out string? stringSource))
		{
			var stringTest = test.Stringify();

			if (stringTest == null || stringSource == null)
				throw new JsonLogicException($"Cannot check string for {test.JsonType()}.");

			return !string.IsNullOrEmpty(stringTest) && stringSource.Contains(stringTest);
		}

		if (source is JsonArray arr)
			return arr.Any(i => i.IsEquivalentTo(test));

		return false;
	}
}

internal class InRuleJsonConverter : AotCompatibleJsonConverter<InRule>
{
	public override InRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.Read(ref reader, JsonLogicSerializerContext.Default.RuleArray);

		if (parameters is not { Length: 2 })
			throw new JsonException("The in rule needs an array with 2 parameters.");

		return new InRule(parameters[0], parameters[1]);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, InRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("in");
		writer.WriteStartArray();
		writer.WriteRule(value.Test, options);
		writer.WriteRule(value.Value, options);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}