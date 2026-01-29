# RpcJsonContextGen

一个用于自动生成 `JsonSerializable` 特性的 C# 代码生成工具，专为配合 System.Text.Json 源生成器和 RPC 服务设计。

## 功能简介

RpcJsonContextGen 可以扫描你的 C# 源代码文件（通常是 RPC 接口定义），自动提取所有方法的参数类型和返回值类型，并生成相应的 `[JsonSerializable(typeof(T))]` 特性代码，然后自动复制到剪贴板。

这在使用 System.Text.Json 的源生成器（Source Generator）进行 AOT 编译时特别有用，可以避免手动维护大量的 JsonSerializable 特性声明。

## 工作原理

1. **解析源代码**：使用简单的字符串解析器分析 C# 源文件，提取公共接口和类中的方法签名
2. **收集类型信息**：遍历所有方法的参数和返回值，收集所有需要序列化的类型
3. **去重和规范化**：对类型名称进行规范化处理（去除 `?`、`global::`、`System.` 前缀等），并去重
4. **生成特性代码**：为每个类型生成 `[JsonSerializable(typeof(T))]` 特性声明
5. **输出到剪贴板**：自动将生成的代码复制到剪贴板（如果失败则输出到控制台）

支持的剪贴板工具：
- Windows: `clip`
- macOS: `pbcopy`
- Linux (Wayland): `wl-copy`
- Linux (X11): `xclip`

## 使用方法

### 方式一：命令行直接使用

```bash
# 分析单个文件
RpcJsonContextGen.exe path/to/YourInterface.cs

# 分析多个文件
RpcJsonContextGen.exe file1.cs file2.cs file3.cs

# 分析整个目录（递归搜索所有 .cs 文件）
RpcJsonContextGen.exe path/to/directory
```

生成的代码会自动复制到剪贴板，然后你可以粘贴到你的 JsonSerializerContext 类中。

### 方式二：配置 Visual Studio 外部工具（推荐）

#### 配置步骤

1. **打开外部工具配置**
   - 在 Visual Studio 菜单栏，点击 `工具` → `外部工具...`

2. **添加新工具**
   - 点击 `添加` 按钮
   - 配置以下信息：

   **标题（T）：**
   ```
   生成 JsonSerializable 特性
   ```

   **命令（C）：**
   ```
   D:\path\to\RpcJsonContextGen.exe
   ```
   *(将路径替换为你实际的 RpcJsonContextGen.exe 位置)*

   **参数（A）：**
   ```
   "$(ItemPath)"
   ```
   *(对当前打开的文件执行)* 
   
   或者使用：
   ```
   "$(ProjectDir)"
   ```
   *(对整个项目目录执行)*

   **初始目录（I）：**
   ```
   $(ProjectDir)
   ```

   **选项：**
   - ☑ 使用输出窗口（U）
   - ☑ 提示输入参数（P）（可选）

3. **保存配置**
   - 点击 `确定` 保存

#### 使用配置好的外部工具

1. 在 Visual Studio 中打开你的 RPC 接口文件（例如 `IYourService.cs`）
2. 点击菜单 `工具` → `生成 JsonSerializable 特性`
3. 工具会自动分析文件并将生成的特性代码复制到剪贴板
4. 在你的 JsonSerializerContext 部分类中粘贴（Ctrl+V）

#### 示例工作流

假设你有以下 RPC 接口：

```csharp
public interface IUserService
{
    Task<UserInfo> GetUserAsync(int userId);
    Task<List<UserInfo>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(UserInfo user);
}
```

1. 打开 `IUserService.cs` 文件
2. 执行外部工具：`工具` → `生成 JsonSerializable 特性`
3. 生成的代码会自动复制到剪贴板：

```csharp
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(List<UserInfo>))]
[JsonSerializable(typeof(Task<bool>))]
[JsonSerializable(typeof(Task<List<UserInfo>>))]
[JsonSerializable(typeof(Task<UserInfo>))]
[JsonSerializable(typeof(UserInfo))]
```

4. 粘贴到你的 JsonSerializerContext 类：

```csharp
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(List<UserInfo>))]
[JsonSerializable(typeof(Task<bool>))]
[JsonSerializable(typeof(Task<List<UserInfo>>))]
[JsonSerializable(typeof(Task<UserInfo>))]
[JsonSerializable(typeof(UserInfo))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class MyJsonContext : JsonSerializerContext
{
}
```

### 方式三：集成到构建流程

你也可以将工具集成到 MSBuild 流程中，在编译前自动生成代码：

```xml
<Target Name="GenerateJsonSerializable" BeforeTargets="BeforeBuild">
  <Exec Command="RpcJsonContextGen.exe $(ProjectDir) > $(ProjectDir)Generated\JsonSerializable.cs" />
</Target>
```

## 编译和发布

### 开发模式

```bash
dotnet build
```

### AOT 发布（推荐）

项目已配置为支持 Native AOT 编译，可生成独立的原生可执行文件：

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

生成的可执行文件位于：`bin/Release/net10.0/win-x64/publish/RpcJsonContextGen.exe`

支持的运行时标识符：
- `win-x64`：Windows 64位
- `linux-x64`：Linux 64位
- `osx-x64`：macOS Intel
- `osx-arm64`：macOS Apple Silicon

## 系统要求

- .NET 10.0 或更高版本（如使用源代码运行）
- 已发布的 AOT 版本无需安装 .NET 运行时

## 注意事项

1. 工具使用简单的字符串解析，不依赖 Roslyn，因此速度快但可能在复杂的泛型或嵌套类型上有限制
2. 自动过滤掉可空标记 `?` 和常见的命名空间前缀（`global::`、`System.`）
3. 如果剪贴板设置失败，代码会输出到控制台
4. 建议对生成的特性列表进行人工审查，移除不需要序列化的类型

## 许可证

根据项目源代码头部注释，本项目版权归若汝棋茗所有，遵循仓库的开源协议。

## 相关链接

- TouchSocket 官网：https://touchsocket.net/
- Gitee：https://gitee.com/RRQM_Home
- GitHub：https://github.com/RRQM

## 技术支持

如有问题，欢迎加入 QQ 交流群：234762506
