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

public class RpcJsonContextGeneratorTests : IDisposable
{
    private readonly string m_testDirectory;

    public RpcJsonContextGeneratorTests()
    {
        m_testDirectory = Path.Combine(Path.GetTempPath(), "RpcJsonContextGenTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(m_testDirectory);
    }

    #region ExpandArgsToFiles Tests

    [Fact]
    public void ExpandArgsToFiles_ShouldHandleEmptyArray()
    {
        var result = RpcJsonContextGenerator.ExpandArgsToFiles([]).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldHandleSingleFile()
    {
        var testFile = Path.Combine(m_testDirectory, "Test.cs");
        File.WriteAllText(testFile, "public class Test { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([testFile]).ToList();

        Assert.Single(result);
        Assert.Equal(testFile, result[0]);

        File.Delete(testFile);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldHandleMultipleFiles()
    {
        var file1 = Path.Combine(m_testDirectory, "Test1.cs");
        var file2 = Path.Combine(m_testDirectory, "Test2.cs");
        File.WriteAllText(file1, "public class Test1 { }");
        File.WriteAllText(file2, "public class Test2 { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([file1, file2]).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(file1, result);
        Assert.Contains(file2, result);

        File.Delete(file1);
        File.Delete(file2);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldExpandDirectory()
    {
        var subDir = Path.Combine(m_testDirectory, "SubDir");
        Directory.CreateDirectory(subDir);
        var file1 = Path.Combine(subDir, "Test1.cs");
        var file2 = Path.Combine(subDir, "Test2.cs");
        File.WriteAllText(file1, "public class Test1 { }");
        File.WriteAllText(file2, "public class Test2 { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([subDir]).ToList();

        Assert.Equal(2, result.Count);

        File.Delete(file1);
        File.Delete(file2);
        Directory.Delete(subDir);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldExpandDirectoryRecursively()
    {
        var subDir1 = Path.Combine(m_testDirectory, "Dir1");
        var subDir2 = Path.Combine(subDir1, "Dir2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        var file1 = Path.Combine(subDir1, "Test1.cs");
        var file2 = Path.Combine(subDir2, "Test2.cs");
        File.WriteAllText(file1, "public class Test1 { }");
        File.WriteAllText(file2, "public class Test2 { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([subDir1]).ToList();

        Assert.Equal(2, result.Count);

        File.Delete(file1);
        File.Delete(file2);
        Directory.Delete(subDir2);
        Directory.Delete(subDir1);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldHandleNonExistentFile()
    {
        var result = RpcJsonContextGenerator.ExpandArgsToFiles(["non_existent_file.cs"]).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldHandleNonExistentDirectory()
    {
        var result = RpcJsonContextGenerator.ExpandArgsToFiles(["non_existent_directory"]).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldTrimQuotes()
    {
        var testFile = Path.Combine(m_testDirectory, "Test.cs");
        File.WriteAllText(testFile, "public class Test { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([$"\"{testFile}\""]).ToList();

        Assert.Single(result);
        Assert.Equal(testFile, result[0]);

        File.Delete(testFile);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldSkipEmptyOrWhitespace()
    {
        var testFile = Path.Combine(m_testDirectory, "Test.cs");
        File.WriteAllText(testFile, "public class Test { }");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles(["", " ", testFile]).ToList();

        Assert.Single(result);
        Assert.Equal(testFile, result[0]);

        File.Delete(testFile);
    }

    [Fact]
    public void ExpandArgsToFiles_ShouldOnlyIncludeCsFiles()
    {
        var subDir = Path.Combine(m_testDirectory, "MixedFiles");
        Directory.CreateDirectory(subDir);

        var csFile = Path.Combine(subDir, "Test.cs");
        var txtFile = Path.Combine(subDir, "Test.txt");
        var jsonFile = Path.Combine(subDir, "Test.json");
        File.WriteAllText(csFile, "public class Test { }");
        File.WriteAllText(txtFile, "text file");
        File.WriteAllText(jsonFile, "{}");

        var result = RpcJsonContextGenerator.ExpandArgsToFiles([subDir]).ToList();

        Assert.Single(result);
        Assert.Contains(csFile, result);

        File.Delete(csFile);
        File.Delete(txtFile);
        File.Delete(jsonFile);
        Directory.Delete(subDir);
    }

    #endregion

    #region GenerateFromFiles Tests

    [Fact]
    public void GenerateFromFiles_ShouldReturnNullForEmptyList()
    {
        var result = RpcJsonContextGenerator.GenerateFromFiles([]);

        Assert.Null(result);
    }

    [Fact]
    public void GenerateFromFiles_ShouldReturnNullForNonExistentFiles()
    {
        var result = RpcJsonContextGenerator.GenerateFromFiles(["non_existent.cs"]);

        Assert.Null(result);
    }

    [Fact]
    public void GenerateFromFiles_ShouldGenerateForSingleFile()
    {
        var testFile = Path.Combine(m_testDirectory, "TestService.cs");
        File.WriteAllText(testFile, @"
public class TestService
{
    public Task<UserDto> GetDataAsync(int id) { return null; }
}

public class UserDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(", result);
        Assert.Contains("UserDto", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(int))", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldGenerateForMultipleFiles()
    {
        var file1 = Path.Combine(m_testDirectory, "Service1.cs");
        var file2 = Path.Combine(m_testDirectory, "Service2.cs");

        File.WriteAllText(file1, @"
public class Service1
{
    public DataDto GetData() { return null; }
}

public class DataDto { }");

        File.WriteAllText(file2, @"
public class Service2
{
    public CountDto GetCount() { return null; }
}

public class CountDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([file1, file2]);

        Assert.NotNull(result);
        Assert.Contains("DataDto", result);
        Assert.Contains("CountDto", result);

        File.Delete(file1);
        File.Delete(file2);
    }

    [Fact]
    public void GenerateFromFiles_ShouldDeduplicateTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "Service.cs");
        File.WriteAllText(testFile, @"
public class Service1
{
    public UserDto Method1() { return null; }
    public UserDto Method2() { return null; }
    public UserDto Method3() { return null; }
}

public class UserDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        var count = System.Text.RegularExpressions.Regex.Matches(
            result,
            @"\[JsonSerializable\(typeof\(UserDto\)\)\]").Count;
        Assert.Equal(1, count);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleEmptyFile()
    {
        var testFile = Path.Combine(m_testDirectory, "Empty.cs");
        File.WriteAllText(testFile, "");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.Null(result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleFileWithoutPublicTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "Internal.cs");
        File.WriteAllText(testFile, @"
internal class InternalClass
{
    private void PrivateMethod() { }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.Null(result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleComplexTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "ComplexService.cs");
        File.WriteAllText(testFile, @"
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyApp.Services
{
    public interface IMyService
    {
        Task<UserDto> GetUserAsync(int userId);
        void UpdateUser(UserDto user);
        List<string> GetNames();
    }

    public class MyService : IMyService
    {
        public Task<UserDto> GetUserAsync(int userId) { return null; }
        public void UpdateUser(UserDto user) { }
        public List<string> GetNames() { return null; }
    }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("JsonSerializable", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleGenericTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "GenericService.cs");
        File.WriteAllText(testFile, @"
public class GenericService
{
    public Task<Dictionary<string, List<int>>> GetComplexData() { return null; }
    public void ProcessData(IEnumerable<KeyValuePair<string, object>> items) { }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("JsonSerializable", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleNullableTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "NullableService.cs");
        File.WriteAllText(testFile, @"
public class NullableService
{
    public DataDto? GetData(NullableDto? id) { return null; }
}

public class DataDto { }
public class NullableDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("DataDto", result);
        Assert.Contains("NullableDto", result);
        Assert.DoesNotContain("?", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleInterfaceMethods()
    {
        var testFile = Path.Combine(m_testDirectory, "IService.cs");
        File.WriteAllText(testFile, @"
public interface IDataService
{
    Task<ValidationResult> ValidateAsync(InputData input);
    void Process(List<ItemDto> items);
}

public class ValidationResult { }
public class InputData { }
public class ItemDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("ValidationResult", result);
        Assert.Contains("InputData", result);
        Assert.Contains("List<ItemDto>", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleMethodsWithAttributes()
    {
        var testFile = Path.Combine(m_testDirectory, "AttributedService.cs");
        File.WriteAllText(testFile, @"
public class AttributedService
{
    [Obsolete]
    public void OldMethod(DataDto data) { }
    
    [HttpPost]
    public Task<ResponseDto> PostAsync([FromBody] RequestDto body) { return null; }
}

public class DataDto { }
public class RequestDto { }
public class ResponseDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("DataDto", result);
        Assert.Contains("RequestDto", result);
        Assert.Contains("ResponseDto", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldSortTypesAlphabetically()
    {
        var testFile = Path.Combine(m_testDirectory, "SortTest.cs");
        File.WriteAllText(testFile, @"
public class SortTest
{
    public void Method(ZebraDto z, YellowDto y, AppleDto x) { }
}

public class ZebraDto { }
public class YellowDto { }
public class AppleDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var appleIndex = Array.FindIndex(lines, l => l.Contains("AppleDto"));
        var yellowIndex = Array.FindIndex(lines, l => l.Contains("YellowDto"));
        var zebraIndex = Array.FindIndex(lines, l => l.Contains("ZebraDto"));

        Assert.True(appleIndex < yellowIndex);
        Assert.True(yellowIndex < zebraIndex);

        File.Delete(testFile);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_ExpandAndGenerate()
    {
        var subDir = Path.Combine(m_testDirectory, "Integration");
        Directory.CreateDirectory(subDir);

        var file1 = Path.Combine(subDir, "Service1.cs");
        var file2 = Path.Combine(subDir, "Service2.cs");

        File.WriteAllText(file1, @"
public class Service1
{
    public Task<DataDto> GetAsync() { return null; }
}

public class DataDto { }");

        File.WriteAllText(file2, @"
public interface IService2
{
    void Update(UpdateRequest request, FlagDto flag);
}

public class UpdateRequest { }
public class FlagDto { }");

        var files = RpcJsonContextGenerator.ExpandArgsToFiles([subDir]).ToList();
        var result = RpcJsonContextGenerator.GenerateFromFiles(files);

        Assert.Equal(2, files.Count);
        Assert.NotNull(result);
        Assert.Contains("DataDto", result);
        Assert.Contains("UpdateRequest", result);
        Assert.Contains("FlagDto", result);

        File.Delete(file1);
        File.Delete(file2);
        Directory.Delete(subDir);
    }

    [Fact]
    public void Integration_MixedFilesAndDirectories()
    {
        var subDir = Path.Combine(m_testDirectory, "Mixed");
        Directory.CreateDirectory(subDir);

        var file1 = Path.Combine(m_testDirectory, "Direct.cs");
        var file2 = Path.Combine(subDir, "InDir.cs");

        File.WriteAllText(file1, @"
public class DirectClass
{
    public DataDto Method1() { return null; }
}

public class DataDto { }");

        File.WriteAllText(file2, @"
public class InDirClass
{
    public CountDto Method2() { return null; }
}

public class CountDto { }");

        var files = RpcJsonContextGenerator.ExpandArgsToFiles([file1, subDir]).ToList();
        var result = RpcJsonContextGenerator.GenerateFromFiles(files);

        Assert.Equal(2, files.Count);
        Assert.NotNull(result);
        Assert.Contains("DataDto", result);
        Assert.Contains("CountDto", result);

        File.Delete(file1);
        File.Delete(file2);
        Directory.Delete(subDir);
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(m_testDirectory))
        {
            try
            {
                Directory.Delete(m_testDirectory, true);
            }
            catch
            {
            }
        }
    }

    #endregion

    #region Task and ValueTask Extraction Tests

    [Fact]
    public void GenerateFromFiles_ShouldExtractInnerTypeFromTask()
    {
        var testFile = Path.Combine(m_testDirectory, "TaskService.cs");
        File.WriteAllText(testFile, @"
public class TaskService
{
    public Task<StringDto> GetStringAsync() { return null; }
    public Task<IntDto> GetIntAsync() { return null; }
}

public class StringDto { }
public class IntDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(StringDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(IntDto))]", result);
        Assert.DoesNotContain("Task<", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExtractInnerTypeFromValueTask()
    {
        var testFile = Path.Combine(m_testDirectory, "ValueTaskService.cs");
        File.WriteAllText(testFile, @"
public class ValueTaskService
{
    public ValueTask<ValidationResult> ValidateAsync() { return default; }
    public ValueTask<CalculationResult> CalculateAsync() { return default; }
}

public class ValidationResult { }
public class CalculationResult { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(ValidationResult))]", result);
        Assert.Contains("[JsonSerializable(typeof(CalculationResult))]", result);
        Assert.DoesNotContain("ValueTask<", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExtractComplexTypeFromTask()
    {
        var testFile = Path.Combine(m_testDirectory, "ComplexTaskService.cs");
        File.WriteAllText(testFile, @"
public class ComplexTaskService
{
    public Task<List<string>> GetListAsync() { return null; }
    public Task<Dictionary<string, int>> GetDictAsync() { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(List<string>))]", result);
        Assert.Contains("[JsonSerializable(typeof(Dictionary<string, int>))]", result);
        Assert.DoesNotContain("Task<List", result);
        Assert.DoesNotContain("Task<Dictionary", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldKeepOtherGenericsIntact()
    {
        var testFile = Path.Combine(m_testDirectory, "GenericParamsService.cs");
        File.WriteAllText(testFile, @"
public class GenericParamsService
{
    public void ProcessList(List<int> items) { }
    public void ProcessDict(Dictionary<string, object> dict) { }
    public void ProcessEnumerable(IEnumerable<KeyValuePair<string, int>> pairs) { }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(Dictionary<string, object>))]", result);
        Assert.Contains("[JsonSerializable(typeof(IEnumerable<KeyValuePair<string, int>>))]", result);
        Assert.Contains("[JsonSerializable(typeof(List<int>))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleMixedAsyncAndNonAsync()
    {
        var testFile = Path.Combine(m_testDirectory, "MixedService.cs");
        File.WriteAllText(testFile, @"
public class MixedService
{
    public Task<DataDto> GetAsync() { return null; }
    public DataDto GetSync() { return null; }
    public void Process(List<ItemDto> items) { }
    public ValueTask<ValidationResult> ValidateAsync() { return default; }
}

public class DataDto { }
public class ItemDto { }
public class ValidationResult { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(ValidationResult))]", result);
        Assert.Contains("[JsonSerializable(typeof(List<ItemDto>))]", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);
        Assert.DoesNotContain("Task<", result);
        Assert.DoesNotContain("ValueTask<", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleTaskWithSystemPrefix()
    {
        var testFile = Path.Combine(m_testDirectory, "SystemPrefixService.cs");
        File.WriteAllText(testFile, @"
public class SystemPrefixService
{
    public System.Threading.Tasks.Task<DataDto> GetAsync() { return null; }
    public System.Threading.Tasks.ValueTask<CountDto> GetIntAsync() { return default; }
}

public class DataDto { }
public class CountDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(CountDto))]", result);
        Assert.DoesNotContain("Task<", result);
        Assert.DoesNotContain("ValueTask<", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleNestedTaskGenerics()
    {
        var testFile = Path.Combine(m_testDirectory, "NestedTaskService.cs");
        File.WriteAllText(testFile, @"
public class NestedTaskService
{
    public Task<List<Dictionary<string, int>>> GetNestedAsync() { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(List<Dictionary<string, int>>))]", result);
        Assert.DoesNotContain("Task<", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleNullableTaskReturnType()
    {
        var testFile = Path.Combine(m_testDirectory, "NullableTaskService.cs");
        File.WriteAllText(testFile, @"
public class NullableTaskService
{
    public Task<NullableDto?> GetNullableIntAsync() { return null; }
    public Task<DataDto?> GetNullableStringAsync() { return null; }
}

public class NullableDto { }
public class DataDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(NullableDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);
        Assert.DoesNotContain("?", result);
        Assert.DoesNotContain("Task<", result);

        File.Delete(testFile);
    }

    #endregion

    #region ExcludedTypes Tests

    [Fact]
    public void GenerateFromFiles_ShouldExcludeVoidType()
    {
        var testFile = Path.Combine(m_testDirectory, "VoidService.cs");
        File.WriteAllText(testFile, @"
public class VoidService
{
    public void Method1() { }
    public void Method2() { }
    public void Method3(DataDto data) { }
}

public class DataDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("[JsonSerializable(typeof(void))]", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExcludeTaskWithoutGenericParameter()
    {
        var testFile = Path.Combine(m_testDirectory, "TaskOnlyService.cs");
        File.WriteAllText(testFile, @"
public class TaskOnlyService
{
    public Task ExecuteAsync() { return null; }
    public ValueTask ProcessAsync() { return default; }
    public Task<DataDto> GetAsync() { return null; }
}

public class DataDto { }");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("[JsonSerializable(typeof(Task))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(ValueTask))]", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);

        File.Delete(testFile);
    }

  
    [Fact]
    public void GenerateFromFiles_ShouldIncludeCustomTypesButExcludeBasicTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "MixedTypesService.cs");
        File.WriteAllText(testFile, @"
public class MixedTypesService
{
    public UserDto GetUser() { return null; }
    public int GetUserId() { return 0; }
    public string GetUserName() { return null; }
    public bool IsValid() { return true; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(int))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(string))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(bool))]", result);

        File.Delete(testFile);
    }

    #endregion

    #region CallContext Exclusion Tests

    [Fact]
    public void GenerateFromFiles_ShouldExcludeCallContextType()
    {
        var testFile = Path.Combine(m_testDirectory, "CallContextService.cs");
        File.WriteAllText(testFile, @"
public class CallContextService
{
    public void Method1(MyCallContext context) { }
    public DataDto Method2(UserCallContext context) { return null; }
}

public class MyCallContext { }
public class UserCallContext { }
public class DataDto { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("MyCallContext", result);
        Assert.DoesNotContain("UserCallContext", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExcludeCallContextWithNamespace()
    {
        var testFile = Path.Combine(m_testDirectory, "NamespacedCallContextService.cs");
        File.WriteAllText(testFile, @"
namespace MyApp.Services
{
    public class ServiceWithContext
    {
        public void Execute(MyApp.Models.ServiceCallContext context) { }
        public UserDto GetUser(RequestCallContext context) { return null; }
    }
}

namespace MyApp.Models
{
    public class ServiceCallContext { }
    public class RequestCallContext { }
    public class UserDto { }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("ServiceCallContext", result);
        Assert.DoesNotContain("RequestCallContext", result);
        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExcludeGenericCallContext()
    {
        var testFile = Path.Combine(m_testDirectory, "GenericCallContextService.cs");
        File.WriteAllText(testFile, @"
public class GenericCallContextService
{
    public void Method1(MyCallContext<string> context) { }
    public void Method2(UserCallContext<int, bool> context) { }
    public UserDto GetUser() { return null; }
}

public class MyCallContext<T> { }
public class UserCallContext<T1, T2> { }
public class UserDto { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("MyCallContext", result);
        Assert.DoesNotContain("UserCallContext", result);
        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldNotExcludeTypeContainingCallContext()
    {
        var testFile = Path.Combine(m_testDirectory, "CallContextInNameService.cs");
        File.WriteAllText(testFile, @"
public class CallContextInNameService
{
    public void Method1(CallContextManager manager) { }
    public void Method2(ServiceCallContextHandler handler) { }
}

public class CallContextManager { }
public class ServiceCallContextHandler { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(CallContextManager))]", result);
        Assert.Contains("[JsonSerializable(typeof(ServiceCallContextHandler))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldExcludeCallContextAsReturnType()
    {
        var testFile = Path.Combine(m_testDirectory, "CallContextReturnService.cs");
        File.WriteAllText(testFile, @"
public class CallContextReturnService
{
    public MyCallContext CreateContext() { return null; }
    public Task<UserCallContext> GetContextAsync() { return null; }
    public DataDto GetData(RequestCallContext context) { return null; }
}

public class MyCallContext { }
public class UserCallContext { }
public class RequestCallContext { }
public class DataDto { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.DoesNotContain("MyCallContext", result);
        Assert.DoesNotContain("UserCallContext", result);
        Assert.DoesNotContain("RequestCallContext", result);
        Assert.Contains("[JsonSerializable(typeof(DataDto))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleMixedCallContextAndNormalTypes()
    {
        var testFile = Path.Combine(m_testDirectory, "MixedCallContextService.cs");
        File.WriteAllText(testFile, @"
public class MixedCallContextService
{
    public UserDto GetUser(UserCallContext context) { return null; }
    public ProductDto GetProduct(int id, RequestCallContext context) { return null; }
    public void Update(UserDto user, UpdateCallContext context) { }
}

public class UserDto { }
public class ProductDto { }
public class UserCallContext { }
public class RequestCallContext { }
public class UpdateCallContext { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(ProductDto))]", result);
        Assert.DoesNotContain("UserCallContext", result);
        Assert.DoesNotContain("RequestCallContext", result);
        Assert.DoesNotContain("UpdateCallContext", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(int))]", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldHandleComplexScenarioWithAllExclusions()
    {
        var testFile = Path.Combine(m_testDirectory, "ComplexExclusionService.cs");
        File.WriteAllText(testFile, @"
public class ComplexExclusionService
{
    public void VoidMethod() { }
    public Task EmptyTaskMethod() { return null; }
    public Task<UserDto> GetUserAsync(int userId, RequestCallContext context) { return null; }
    public ValueTask<ProductDto> GetProductAsync(string productId) { return default; }
    public string GetName() { return null; }
    public DateTime GetDate() { return default; }
    public bool IsValid(ValidationCallContext context) { return true; }
    public CustomDto GetCustom() { return null; }
}

public class UserDto { }
public class ProductDto { }
public class CustomDto { }
public class RequestCallContext { }
public class ValidationCallContext { }
");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        
        // 应该包含的类型
        Assert.Contains("[JsonSerializable(typeof(UserDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(ProductDto))]", result);
        Assert.Contains("[JsonSerializable(typeof(CustomDto))]", result);
        
        // 不应该包含的类型
        Assert.DoesNotContain("[JsonSerializable(typeof(void))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(Task))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(int))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(string))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(DateTime))]", result);
        Assert.DoesNotContain("[JsonSerializable(typeof(bool))]", result);
        Assert.DoesNotContain("RequestCallContext", result);
        Assert.DoesNotContain("ValidationCallContext", result);

        File.Delete(testFile);
    }

    #endregion
}






