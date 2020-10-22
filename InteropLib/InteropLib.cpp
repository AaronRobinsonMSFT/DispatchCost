#include <atlbase.h>

using namespace System;

public ref class InteropInvoker
{
public:
    void SetInvoker(void* managedInst);
    void InvokeWithObject(Object^ id);
    void InvokeWithInt(int id);
};

//GUID IID_IInvoker = { 0xed9e0067, 0xf920, 0x4b8a, {0xae, 0xc1, 0x07, 0x1d, 0xc6, 0xa8, 0x26, 0xac } };
struct __declspec(uuid("ed9e0067-f920-4b8a-aec1-071dc6a826ac")) IInvoker : IUnknown
{
    STDMETHOD(InvokeWithObject)(VARIANT) = 0;
    STDMETHOD(InvokeWithInt)(int) = 0;
};

CComPtr<::IInvoker> comInvoker;

void InteropInvoker::SetInvoker(void* managedInst)
{
    auto pUnk = reinterpret_cast<IUnknown*>(managedInst);

    HRESULT hr = pUnk->QueryInterface(&comInvoker);
    if (FAILED(hr))
        std::abort();
}

void InteropInvoker::InvokeWithObject(Object^ arg)
{
    HRESULT hr;

    CComVariant tmp;
    System::IntPtr varIntPtr(&tmp);
    System::Runtime::InteropServices::Marshal::GetNativeVariantForObject(arg, varIntPtr);
    hr = comInvoker->InvokeWithObject(tmp);
    System::Runtime::InteropServices::Marshal::ThrowExceptionForHR(hr);
}

void InteropInvoker::InvokeWithInt(int arg)
{
    HRESULT hr;

    hr = comInvoker->InvokeWithInt(arg);
    System::Runtime::InteropServices::Marshal::ThrowExceptionForHR(hr);
}
