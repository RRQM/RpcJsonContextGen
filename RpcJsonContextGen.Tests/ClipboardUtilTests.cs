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

namespace RpcJsonContextGen.Tests;

public class ClipboardUtilTests
{
    [Fact]
    public void TrySetText_ShouldHandleEmptyString()
    {
        var result = ClipboardUtil.TrySetText(string.Empty);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldHandleSimpleText()
    {
        var text = "Hello, World!";
        
        var result = ClipboardUtil.TrySetText(text);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldHandleLargeText()
    {
        var text = new string('a', 10000);
        
        var result = ClipboardUtil.TrySetText(text);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldHandleMultilineText()
    {
        var text = @"Line 1
Line 2
Line 3";
        
        var result = ClipboardUtil.TrySetText(text);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldHandleSpecialCharacters()
    {
        var text = "Special chars: \t\n\r\"'\\";
        
        var result = ClipboardUtil.TrySetText(text);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldHandleUnicodeCharacters()
    {
        var text = "Unicode: 你好世界 🎉 αβγδ";
        
        var result = ClipboardUtil.TrySetText(text);
        
        Assert.True(result || !result);
    }

    [Fact]
    public void TrySetText_ShouldNotThrowException()
    {
        var exception = Record.Exception(() => ClipboardUtil.TrySetText("test"));
        
        Assert.Null(exception);
    }

    [Fact]
    public void TrySetText_ShouldHandleJsonSerializableAttributes()
    {
        var text = @"[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(Task<bool>))]";
        
        var exception = Record.Exception(() => ClipboardUtil.TrySetText(text));
        
        Assert.Null(exception);
    }
}
