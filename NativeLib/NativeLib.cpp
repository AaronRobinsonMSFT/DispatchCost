#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <atlbase.h>

//GUID IID_IInvoker = { 0xed9e0067, 0xf920, 0x4b8a, {0xae, 0xc1, 0x07, 0x1d, 0xc6, 0xa8, 0x26, 0xac } };
struct __declspec(uuid("ed9e0067-f920-4b8a-aec1-071dc6a826ac")) IInvoker : IUnknown
{
    STDMETHOD(InvokeWithObject)(VARIANT) = 0;
    STDMETHOD(InvokeWithInt)(int) = 0;
};

CComPtr<::IInvoker> comInvoker;

extern "C" __declspec(dllexport) void SetInvoker(void* ptr)
{
    auto pUnk = reinterpret_cast<IUnknown*>(ptr);

    HRESULT hr = pUnk->QueryInterface(&comInvoker);
    if (FAILED(hr))
        std::abort();
}

extern "C" __declspec(dllexport) int InvokeWithObject(VARIANT arg)
{
    return (int)comInvoker->InvokeWithObject(arg);
}

extern "C" __declspec(dllexport) int InvokeWithInt(int arg)
{
    return (int)comInvoker->InvokeWithInt(arg);
}

using Callback_t = void (STDMETHODCALLTYPE*)(void*, int);
Callback_t Callback;
void* ManagedObj;

extern "C" __declspec(dllexport) void SetInvokerObject(Callback_t fptr, void* obj)
{
    Callback = fptr;
    ManagedObj = obj;
}

extern "C" __declspec(dllexport) void InvokeWithIntFast(int arg)
{
    Callback(ManagedObj, arg);
}
