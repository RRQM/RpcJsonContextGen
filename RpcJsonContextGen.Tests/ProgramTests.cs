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
    public Task<string> GetDataAsync(int id) { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(", result);
        Assert.Contains("string", result);
        Assert.Contains("int", result);

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
    public string GetData() { return null; }
}");

        File.WriteAllText(file2, @"
public class Service2
{
    public int GetCount() { return 0; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([file1, file2]);

        Assert.NotNull(result);
        Assert.Contains("int", result);
        Assert.Contains("string", result);

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
    public string Method1() { return null; }
    public string Method2() { return null; }
    public string Method3() { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        var count = System.Text.RegularExpressions.Regex.Matches(
            result,
            @"\[JsonSerializable\(typeof\(string\)\)\]").Count;
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
    public string? GetData(int? id) { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("string", result);
        Assert.Contains("int", result);
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
    Task<bool> ValidateAsync(string input);
    void Process(List<int> items);
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("bool", result);
        Assert.Contains("string", result);
        Assert.Contains("List<int>", result);

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
    public void OldMethod(string data) { }
    
    [HttpPost]
    public Task<bool> PostAsync([FromBody] string body) { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("string", result);
        Assert.Contains("bool", result);

        File.Delete(testFile);
    }

    [Fact]
    public void GenerateFromFiles_ShouldSortTypesAlphabetically()
    {
        var testFile = Path.Combine(m_testDirectory, "SortTest.cs");
        File.WriteAllText(testFile, @"
public class SortTest
{
    public void Method(string z, int y, bool x) { }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var boolIndex = Array.FindIndex(lines, l => l.Contains("bool"));
        var intIndex = Array.FindIndex(lines, l => l.Contains("int"));
        var stringIndex = Array.FindIndex(lines, l => l.Contains("string"));

        Assert.True(boolIndex < intIndex);
        Assert.True(intIndex < stringIndex);

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
    public Task<string> GetAsync() { return null; }
}");

        File.WriteAllText(file2, @"
public interface IService2
{
    void Update(int id, bool flag);
}");

        var files = RpcJsonContextGenerator.ExpandArgsToFiles([subDir]).ToList();
        var result = RpcJsonContextGenerator.GenerateFromFiles(files);

        Assert.Equal(2, files.Count);
        Assert.NotNull(result);
        Assert.Contains("string", result);
        Assert.Contains("int", result);
        Assert.Contains("bool", result);

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
    public string Method1() { return null; }
}");

        File.WriteAllText(file2, @"
public class InDirClass
{
    public int Method2() { return 0; }
}");

        var files = RpcJsonContextGenerator.ExpandArgsToFiles([file1, subDir]).ToList();
        var result = RpcJsonContextGenerator.GenerateFromFiles(files);

        Assert.Equal(2, files.Count);
        Assert.NotNull(result);
        Assert.Contains("string", result);
        Assert.Contains("int", result);

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
    public Task<string> GetStringAsync() { return null; }
    public Task<int> GetIntAsync() { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(string))]", result);
        Assert.Contains("[JsonSerializable(typeof(int))]", result);
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
    public ValueTask<bool> ValidateAsync() { return default; }
    public ValueTask<double> CalculateAsync() { return default; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(bool))]", result);
        Assert.Contains("[JsonSerializable(typeof(double))]", result);
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
    public Task<string> GetAsync() { return null; }
    public string GetSync() { return null; }
    public void Process(List<int> items) { }
    public ValueTask<bool> ValidateAsync() { return default; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(bool))]", result);
        Assert.Contains("[JsonSerializable(typeof(List<int>))]", result);
        Assert.Contains("[JsonSerializable(typeof(string))]", result);
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
    public System.Threading.Tasks.Task<System.String> GetAsync() { return null; }
    public System.Threading.Tasks.ValueTask<System.Int32> GetIntAsync() { return default; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(String))]", result);
        Assert.Contains("[JsonSerializable(typeof(Int32))]", result);
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
    public Task<int?> GetNullableIntAsync() { return null; }
    public Task<string?> GetNullableStringAsync() { return null; }
}");

        var result = RpcJsonContextGenerator.GenerateFromFiles([testFile]);

        Assert.NotNull(result);
        Assert.Contains("[JsonSerializable(typeof(int))]", result);
        Assert.Contains("[JsonSerializable(typeof(string))]", result);
        Assert.DoesNotContain("?", result);
        Assert.DoesNotContain("Task<", result);

        File.Delete(testFile);
    }

    #endregion
}



