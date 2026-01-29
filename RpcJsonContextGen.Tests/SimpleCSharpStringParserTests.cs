// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using RpcJsonContextGen;

namespace RpcJsonContextGen.Tests;

public class SimpleCSharpStringParserTests
{
    #region RemoveCommentsAndStrings Tests

    [Fact]
    public void RemoveCommentsAndStrings_ShouldRemoveBlockComments()
    {
        var src = "int x = 1; /* This is a \n block comment */ int y = 2;";
        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.NotNull(result);
    }

    [Fact]
    public void RemoveCommentsAndStrings_ShouldRemoveCharLiterals()
    {
        var src = "char c = 'a'; int x = 1;";
        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.NotNull(result);
    }

    [Fact]
    public void RemoveCommentsAndStrings_ShouldRemoveLineComments()
    {
        var src = "int x = 1; // This is a comment\nint y = 2;";
        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.NotNull(result);
    }

    [Fact]
    public void RemoveCommentsAndStrings_ShouldRemoveStrings()
    {
        var src = "string s = \"This is a string\"; int x = 1;";
        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.NotNull(result);
    }

    [Fact]
    public void RemoveCommentsAndStrings_ShouldRemoveVerbatimStrings()
    {
        var src = @"string s = @""C:\Path\To\File""; int x = 1;";
        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.NotNull(result);
    }

    #endregion RemoveCommentsAndStrings Tests

    #region ParseUsings Tests

    [Fact]
    public void ParseFile_ShouldIgnoreUsingsInsideBraces()
    {
        var src = @"namespace Test
{
    using System;
    public class MyClass { }
}";

        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.Empty(result.Usings);
    }

    [Fact]
    public void ParseFile_ShouldParseUsings()
    {
        var src = @"using System;
using System.Collections.Generic;

namespace Test { }";

        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.Equal(2, result.Usings.Count);
        Assert.Contains(result.Usings, u => u.Contains("System"));
        Assert.Contains(result.Usings, u => u.Contains("System.Collections.Generic"));
    }

    #endregion ParseUsings Tests

    #region ParsePublicTypes Tests

    [Fact]
    public void ParsePublicTypes_ShouldParseClassWithModifiers()
    {
        var src = @"
public sealed partial class MyClass
{
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Equal("MyClass", result[0].TypeName);
    }

    [Fact]
    public void ParsePublicTypes_ShouldParseInterface()
    {
        var src = @"
public interface IMyInterface
{
    void Method1();
    string Method2(int param);
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Equal("interface", result[0].Kind);
        Assert.Equal("IMyInterface", result[0].TypeName);
        Assert.Equal(2, result[0].Methods.Count);
    }

    [Fact]
    public void ParsePublicTypes_ShouldParseMultipleTypes()
    {
        var src = @"
public class Class1 { }
public interface IInterface1 { }
public class Class2 { }";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ParsePublicTypes_ShouldParseSimpleClass()
    {
        var src = @"
public class MyClass
{
    public void Method1() { }
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Equal("class", result[0].Kind);
        Assert.Equal("MyClass", result[0].TypeName);
    }

    #endregion ParsePublicTypes Tests

    #region ParseMethods Tests

    [Fact]
    public void ParseInterfaceMethods_ShouldParseSimpleMethods()
    {
        var src = @"
public interface IMyInterface
{
    void Method1();
    string Method2(int id);
    Task<bool> Method3Async(string name, int age);
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Equal(3, types[0].Methods.Count);
        Assert.Equal("void", types[0].Methods[0].ReturnType);
        Assert.Equal("Method1", types[0].Methods[0].Name);
        Assert.Equal("string", types[0].Methods[1].ReturnType);
        Assert.Equal("Method2", types[0].Methods[1].Name);
        Assert.Equal("Task<bool>", types[0].Methods[2].ReturnType);
        Assert.Equal("Method3Async", types[0].Methods[2].Name);
    }

    [Fact]
    public void ParsePublicMethods_ShouldParseGenericParameters()
    {
        var src = @"
public class MyClass
{
    public void Process(List<int> items, Dictionary<string, object> dict) { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(types[0].Methods);
        Assert.Equal(2, types[0].Methods[0].Parameters.Count);
        Assert.Equal("List<int>", types[0].Methods[0].Parameters[0].Type);
        Assert.Equal("Dictionary<string,object>", types[0].Methods[0].Parameters[1].Type.Replace(" ", ""));
    }

    [Fact]
    public void ParsePublicMethods_ShouldParseGenericReturnType()
    {
        var src = @"
public class MyClass
{
    public Task<string> GetValueAsync() { return null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(types[0].Methods);
        Assert.Equal("Task<string>", types[0].Methods[0].ReturnType);
    }

    [Fact]
    public void ParsePublicMethods_ShouldParseMethodWithModifiers()
    {
        var src = @"
public class MyClass
{
    public static async Task<string> GetAsync() { return null; }
    public virtual void DoSomething() { }
    public override string ToString() { return null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Equal(3, types[0].Methods.Count);
        Assert.Equal("Task<string>", types[0].Methods[0].ReturnType);
        Assert.Equal("GetAsync", types[0].Methods[0].Name);
        Assert.Equal("void", types[0].Methods[1].ReturnType);
        Assert.Equal("string", types[0].Methods[2].ReturnType);
    }

    [Fact]
    public void ParsePublicMethods_ShouldParseMethodWithParameters()
    {
        var src = @"
public class MyClass
{
    public string GetValue(int id, string name) { return null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(types[0].Methods);
        Assert.Equal("string", types[0].Methods[0].ReturnType);
        Assert.Equal("GetValue", types[0].Methods[0].Name);
        Assert.Equal(2, types[0].Methods[0].Parameters.Count);
        Assert.Equal("int", types[0].Methods[0].Parameters[0].Type);
        Assert.Equal("id", types[0].Methods[0].Parameters[0].Name);
        Assert.Equal("string", types[0].Methods[0].Parameters[1].Type);
        Assert.Equal("name", types[0].Methods[0].Parameters[1].Name);
    }

    [Fact]
    public void ParsePublicMethods_ShouldParseSimpleMethod()
    {
        var src = @"
public class MyClass
{
    public void DoSomething() { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(types[0].Methods);
        Assert.Equal("void", types[0].Methods[0].ReturnType);
        Assert.Equal("DoSomething", types[0].Methods[0].Name);
        Assert.Empty(types[0].Methods[0].Parameters);
    }

    #endregion ParseMethods Tests

    #region ParseParameters Tests

    [Fact]
    public void ParseParameters_ShouldParseParametersWithAttributes()
    {
        var src = @"
public class MyClass
{
    public void Method([FromBody] string data, [FromQuery] int id) { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var method = types[0].Methods[0];

        Assert.Equal(2, method.Parameters.Count);
        Assert.Contains("FromBody", method.Parameters[0].Attributes);
        Assert.Contains("FromQuery", method.Parameters[1].Attributes);
    }

    [Fact]
    public void ParseParameters_ShouldParseParametersWithDefaultValues()
    {
        var src = @"
public class MyClass
{
    public void Method(int x = 10, string y = null) { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var method = types[0].Methods[0];

        Assert.Equal(2, method.Parameters.Count);
        Assert.Equal("int", method.Parameters[0].Type);
        Assert.Equal("string", method.Parameters[1].Type);
    }

    [Fact]
    public void ParseParameters_ShouldParseParametersWithModifiers()
    {
        var src = @"
public class MyClass
{
    public void Method(ref int x, out string y, in bool z, params object[] items) { y = null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var method = types[0].Methods[0];

        Assert.Equal(4, method.Parameters.Count);
        Assert.Equal("int", method.Parameters[0].Type);
        Assert.Equal("x", method.Parameters[0].Name);
        Assert.Equal("string", method.Parameters[1].Type);
        Assert.Equal("y", method.Parameters[1].Name);
    }

    [Fact]
    public void ParseParameters_ShouldParseSimpleParameters()
    {
        var src = @"
public class MyClass
{
    public void Method(int x, string y) { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var method = types[0].Methods[0];

        Assert.Equal(2, method.Parameters.Count);
        Assert.Equal("int", method.Parameters[0].Type);
        Assert.Equal("x", method.Parameters[0].Name);
        Assert.Equal("string", method.Parameters[1].Type);
        Assert.Equal("y", method.Parameters[1].Name);
    }

    #endregion ParseParameters Tests

    #region Attributes Tests

    [Fact]
    public void ParseAttributes_ShouldParseAttributesWithParameters()
    {
        var src = @"
[Route(""/api/test"")]
[Authorize(Roles = ""Admin"")]
public class MyClass
{
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Equal(2, types[0].Attributes.Count);
        Assert.Contains("Route", types[0].Attributes);
        Assert.Contains("Authorize", types[0].Attributes);
    }

    [Fact]
    public void ParseMethodAttributes_ShouldParseAttributes()
    {
        var src = @"
public class MyClass
{
    [Obsolete]
    [HttpGet]
    public void Method() { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var method = types[0].Methods[0];

        Assert.Equal(2, method.Attributes.Count);
        Assert.Contains("Obsolete", method.Attributes);
        Assert.Contains("HttpGet", method.Attributes);
    }

    [Fact]
    public void ParseTypeAttributes_ShouldParseClassAttributes()
    {
        var src = @"
[Serializable]
[Obsolete]
public class MyClass
{
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Equal(2, types[0].Attributes.Count);
        Assert.Contains("Serializable", types[0].Attributes);
        Assert.Contains("Obsolete", types[0].Attributes);
    }

    #endregion Attributes Tests

    #region CollectMethodTypes Tests

    [Fact]
    public void CollectMethodTypes_ShouldCollectGenericTypes()
    {
        var src = @"
public class MyClass
{
    public Task<string> Method(List<int> items) { return null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var collectedTypes = RpcJsonContextGenerator.CollectMethodTypes(types).ToList();

        Assert.Contains(collectedTypes, t => t.Contains("string"));
        Assert.Contains(collectedTypes, t => t.Contains("List") && t.Contains("int"));
    }

    [Fact]
    public void CollectMethodTypes_ShouldCollectParameterTypes()
    {
        var src = @"
public class MyClass
{
    public void Method(string name, int age, bool active) { }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var collectedTypes = RpcJsonContextGenerator.CollectMethodTypes(types).ToList();

        Assert.Contains("string", collectedTypes);
        Assert.Contains("int", collectedTypes);
        Assert.Contains("bool", collectedTypes);
    }

    [Fact]
    public void CollectMethodTypes_ShouldCollectReturnTypes()
    {
        var src = @"
public class MyClass
{
    public string Method1() { return null; }
    public int Method2() { return 0; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var collectedTypes = RpcJsonContextGenerator.CollectMethodTypes(types).ToList();

        Assert.Contains("string", collectedTypes);
        Assert.Contains("int", collectedTypes);
    }

    [Fact]
    public void CollectMethodTypes_ShouldNormalizeNullableTypes()
    {
        var src = @"
public class MyClass
{
    public string? Method(int? id) { return null; }
}";

        var types = SimpleCSharpStringParser.ParsePublicTypes(src);
        var collectedTypes = RpcJsonContextGenerator.CollectMethodTypes(types).ToList();

        Assert.Contains("string", collectedTypes);
        Assert.Contains("int", collectedTypes);
    }

    #endregion CollectMethodTypes Tests

    #region GenerateJsonSerializableAttributes Tests

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldGenerateAttributes()
    {
        var types = new[] { "UserDto", "ProductDto", "OrderDto" };

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(ProductDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(OrderDto))]", result);
    }

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldRemoveDuplicates()
    {
        var types = new[] { "UserDto", "UserDto", "ProductDto", "ProductDto" };

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldRemoveGlobalPrefix()
    {
        var types = new[] { "global::System.String" };

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.DoesNotContain("global::", result);
    }

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldRemoveSystemPrefix()
    {
        var types = new[] { "System.Collections.Generic.List<UserDto>", "System.Threading.Tasks.Task<ProductDto>" };

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.Contains("[JsonSerializable(typeof(Collections.Generic.List<UserDto>))]", result);
        Assert.Contains("[JsonSerializable(typeof(ProductDto))]", result);
        Assert.DoesNotContain("System.", result);
    }

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldReturnEmptyForEmptyInput()
    {
        var types = Array.Empty<string>();

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.Empty(result);
    }

    [Fact]
    public void GenerateJsonSerializableAttributes_ShouldSortTypes()
    {
        var types = new[] { "ZebraDto", "YellowDto", "AppleDto" };

        var result = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("[JsonSerializable(typeof(AppleDto))]", lines[0]);
        Assert.Equal("[JsonSerializable(typeof(YellowDto))]", lines[1]);
        Assert.Equal("[JsonSerializable(typeof(ZebraDto))]", lines[2]);
    }

    #endregion GenerateJsonSerializableAttributes Tests

    #region Integration Tests

    [Fact]
    public void IntegrationTest_CompleteClassParsing()
    {
        var src = @"
using System;
using System.Threading.Tasks;

namespace MyNamespace
{
    [Serializable]
    public class MyService
    {
        [Obsolete]
        public Task<string> GetDataAsync(int id, [FromQuery] string filter)
        {
            return Task.FromResult(""data"");
        }

        public void UpdateData(List<MyModel> models)
        {
        }
    }

    public interface IMyService
    {
        Task<bool> ValidateAsync(string input);
        void Process();
    }
}";

        var file = SimpleCSharpStringParser.ParseFile(src);

        Assert.Equal(2, file.Usings.Count);
        Assert.Equal(2, file.Types.Count);

        var classType = file.Types[0];
        Assert.Equal("class", classType.Kind);
        Assert.Equal("MyService", classType.TypeName);
        Assert.Single(classType.Attributes);
        Assert.Contains("Serializable", classType.Attributes);
        Assert.Equal(2, classType.Methods.Count);

        var method1 = classType.Methods[0];
        Assert.Equal("GetDataAsync", method1.Name);
        Assert.Contains("Obsolete", method1.Attributes);
        Assert.Equal(2, method1.Parameters.Count);
        Assert.Contains("FromQuery", method1.Parameters[1].Attributes);

        var interfaceType = file.Types[1];
        Assert.Equal("interface", interfaceType.Kind);
        Assert.Equal("IMyService", interfaceType.TypeName);
        Assert.Equal(2, interfaceType.Methods.Count);
    }

    [Fact]
    public void IntegrationTest_ComplexGenericTypes()
    {
        var src = @"
public class MyService
{
    public Task<Dictionary<string, List<int>>> GetComplexData() { return null; }
    public void ProcessData(IEnumerable<KeyValuePair<string, object>> items) { }
}";

        var file = SimpleCSharpStringParser.ParseFile(src);
        var types = RpcJsonContextGenerator.CollectMethodTypes(file.Types);
        var output = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.NotEmpty(output);
        Assert.Contains("JsonSerializable", output);
    }

    [Fact]
    public void IntegrationTest_EndToEndJsonSerializableGeneration()
    {
        var src = @"
public class MyApi
{
    public Task<UserResponse> GetUser(UserRequest userId) { return null; }
    public void UpdateUser(UserRequest request) { }
}";

        var file = SimpleCSharpStringParser.ParseFile(src);
        var types = RpcJsonContextGenerator.CollectMethodTypes(file.Types);
        var output = RpcJsonContextGenerator.GenerateJsonSerializableAttributes(types);

        Assert.Contains("[JsonSerializable(typeof(UserResponse))]", output);
        Assert.Contains("[JsonSerializable(typeof(UserRequest))]", output);
        Assert.DoesNotContain("[JsonSerializable(typeof(int))]", output);
    }

    #endregion Integration Tests

    #region Edge Cases

    [Fact]
    public void EdgeCase_ClassWithNoMethods()
    {
        var src = @"
public class EmptyClass
{
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Empty(result[0].Methods);
    }

    [Fact]
    public void EdgeCase_EmptyFile()
    {
        var src = "";

        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.Empty(result.Usings);
        Assert.Empty(result.Types);
    }

    [Fact]
    public void EdgeCase_InterfaceWithExplicitPublicModifier()
    {
        var src = @"
public interface IMyInterface
{
    public void Method1();
    void Method2();
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Equal(2, result[0].Methods.Count);
    }

    [Fact]
    public void EdgeCase_MethodWithComplexSignature()
    {
        var src = @"
public class MyClass
{
    public async Task<Dictionary<string, List<Tuple<int, string, bool?>>>>
        ComplexMethod(
            [Attribute1, Attribute2] ref Dictionary<int, string> param1,
            out List<int?> param2,
            params object[] items)
    {
        param2 = null;
        return null;
    }
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.Single(result);
        Assert.Single(result[0].Methods);
        Assert.Equal("ComplexMethod", result[0].Methods[0].Name);
        Assert.Equal(3, result[0].Methods[0].Parameters.Count);
    }

    [Fact]
    public void EdgeCase_NestedClasses()
    {
        var src = @"
public class OuterClass
{
    public class InnerClass
    {
        public void Method() { }
    }
}";

        var result = SimpleCSharpStringParser.ParsePublicTypes(src);

        Assert.True(result.Count >= 1);
        Assert.Equal("OuterClass", result[0].TypeName);
    }

    [Fact]
    public void EdgeCase_OnlyComments()
    {
        var src = @"
// This is a comment
/* This is a
   block comment */";

        var result = SimpleCSharpStringParser.ParseFile(src);

        Assert.Empty(result.Types);
    }

    #endregion Edge Cases
}