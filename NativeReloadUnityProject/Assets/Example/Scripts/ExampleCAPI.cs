﻿using System;
using System.Runtime.InteropServices;

using fts_plugin_loader;

// ------------------------------------------------------------------------
// Example C API defined in my_cool_plugin.h
// ------------------------------------------------------------------------
/*
extern "C" 
{
	__declspec(dllexport) int simple_func();
    __declspec(dllexport) float sum(float a, float b);
    __declspec(dllexport) int string_length(const char* s);

    struct SimpleStruct
    {
        int a;
        float b;
        bool c;
    };
    __declspec(dllexport) double RecvStruct(SimpleStruct const* ss);
    __declspec(dllexport) SimpleStruct SendStruct();
}
*/


// ------------------------------------------------------------------------
// Basic PInvoke
// ------------------------------------------------------------------------
public static class FooPlugin_PInvoke
{
    [DllImport("cpp_example_dll", EntryPoint = "simple_func")]
    extern static public int test_func();
}


// ------------------------------------------------------------------------
// (Manual) Lazy lookup
//
// Here's an example of how to do a slightly more manual Lazy lookup
// ------------------------------------------------------------------------
[PluginAttr("cpp_example_dll", lazy: true)]
public static class FooPluginAPI_Lazy
{
    const string pluginName = "cpp_example_dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int TestFunc();
    static TestFunc _testFunc = null;
    public static TestFunc testFunc {
        get {
            if (_testFunc == null) {
                var fn = NativePluginLoader.GetPlugin(pluginName).GetFunction("simple_func");
                _testFunc = Marshal.GetDelegateForFunctionPointer<TestFunc>(fn);
            }
            return _testFunc;
        }
    }

    // Plugin wrapper
    static NativePlugin _plugin;
    static NativePlugin plugin {
        get {
            if (_plugin == null)
                _plugin = NativePluginLoader.GetPlugin(pluginName);
            return _plugin;
        }
    }

    static int Test()
    {
        return simpleFunc();
    }

    [PluginLazyFunctionAttr("cpp_example_dll", "simple_func")]
    static LazyFn<SimpleFunc> _simpleFunc = new LazyFn<SimpleFunc>();
    public delegate int SimpleFunc();
    public static SimpleFunc simpleFunc { get { return _simpleFunc.fn; } } 

    class LazyFn<DelegateT> {
        DelegateT _function;

        public DelegateT fn { 
            get {
                if (_function == null) {
                    var attr = GetType().GetCustomAttributes(typeof(PluginLazyFunctionAttr), true)[0] as PluginLazyFunctionAttr;
                    var fn_ptr = NativePluginLoader.GetPlugin(attr.pluginName).GetFunction(attr.functionName);
                    _function = Marshal.GetDelegateForFunctionPointer<DelegateT>(fn_ptr);
                }
                return _function;
            }
        }
    }
}

// ------------------------------------------------------------------------
// Auto Lookup
//
// Requires 'NativePluginLoader' object to exist in scene
// ------------------------------------------------------------------------
[PluginAttr("cpp_example_dll")]
public static class FooPluginAPI_Auto
{
    [PluginFunctionAttr("simple_func")] 
    public static SimpleFunc simpleFunc = null;
    public delegate int SimpleFunc();

    [PluginFunctionAttr("sum")] 
    public static Sum sum = null;
    public delegate float Sum(float a, float b);

    [PluginFunctionAttr("string_length")] 
    public static StringLength stringLength = null;
    public delegate int StringLength([MarshalAs(UnmanagedType.LPStr)]string s);

    [PluginFunctionAttr("send_struct")] 
    public static SendStruct sendStruct = null;
    public delegate double SendStruct(ref SimpleStruct ss);

    [PluginFunctionAttr("recv_struct")]
    public static RecvStruct recvStruct = null;
    public delegate SimpleStruct RecvStruct();

    [StructLayout(LayoutKind.Sequential)]
    public struct SimpleStruct {
        public int a;
        public float b;
        public bool c;

        public SimpleStruct(int a, float b, bool c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
