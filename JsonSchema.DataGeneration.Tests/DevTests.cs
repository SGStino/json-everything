using System;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public class DevTests
	{
		[Test]
		public void Test()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(0)
				.Maximum(100)
				.Not(new JsonSchemaBuilder().MultipleOf(3));

			var result = schema.GenerateData();

			Console.WriteLine(result.Result.ToJsonString());

			Assert.IsTrue(schema.Validate(result.Result).IsValid);
		}
	}
}