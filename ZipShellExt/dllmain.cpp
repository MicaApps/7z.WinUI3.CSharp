#include "pch.h"
#include "BaseExplorerCommand.h"
#include <mutex>

// 导出的 GUID 值
struct DECLSPEC_UUID("B3BFA54C-DA02-5983-38BF-369E77A27B6A") ClassFactory : winrt::implements<ClassFactory, IClassFactory>
{
    HRESULT __stdcall CreateInstance(
        IUnknown * outer,
        GUID const& iid,
        void** result) noexcept final
    {
        *result = nullptr;

        if (outer)
        {
            return CLASS_E_NOAGGREGATION;
        }

        winrt::com_ptr<winrt::ZipShellExt::implementation::SevenZipCommand> sevenZipCommand = winrt::make_self<winrt::ZipShellExt::implementation::SevenZipCommand>();
        winrt::hresult convertResult = sevenZipCommand.as(winrt::guid_of<IExplorerCommand>(), result);
        return S_OK;
    }

    HRESULT __stdcall LockServer(BOOL) noexcept final
    {
        return S_OK;
    }
};

bool __stdcall winrt_can_unload_now() noexcept
{
    if (winrt::get_module_lock())
    {
        return false;
    }

    winrt::clear_factory_cache();
    return true;
}

STDAPI DllCanUnloadNow()
{
    return winrt_can_unload_now() ? S_OK : S_FALSE;
}

STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _COM_Outptr_ void** instance)
{
    return winrt::make<ClassFactory>().as(riid, instance);
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

