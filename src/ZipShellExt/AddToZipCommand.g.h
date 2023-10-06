// WARNING: Please don't edit this file. It was generated by C++/WinRT v2.0.230706.1

#pragma once
#include "winrt/ZipShellExt.h"
namespace winrt::ZipShellExt::implementation
{
    template <typename D, typename B, typename... I>
    struct WINRT_IMPL_EMPTY_BASES AddToZipCommand_base : implements<D, ZipShellExt::AddToZipCommand, B, no_module_lock, I...>
    {
        using base_type = AddToZipCommand_base;
        using class_type = ZipShellExt::AddToZipCommand;
        using implements_type = typename AddToZipCommand_base::implements_type;
        using implements_type::implements_type;
        using composable_base = B;
        hstring GetRuntimeClassName() const
        {
            return L"ZipShellExt.AddToZipCommand";
        }
    };
}
namespace winrt::ZipShellExt::factory_implementation
{
    template <typename D, typename T, typename... I>
    struct WINRT_IMPL_EMPTY_BASES AddToZipCommandT : implements<D, winrt::Windows::Foundation::IActivationFactory, I...>
    {
        using instance_type = ZipShellExt::AddToZipCommand;

        hstring GetRuntimeClassName() const
        {
            return L"ZipShellExt.AddToZipCommand";
        }
        auto ActivateInstance() const
        {
            return make<T>();
        }
    };
}

#if defined(WINRT_FORCE_INCLUDE_ADDTOZIPCOMMAND_XAML_G_H) || __has_include("AddToZipCommand.xaml.g.h")

#include "AddToZipCommand.xaml.g.h"

#else

namespace winrt::ZipShellExt::implementation
{
    template <typename D, typename... I>
    using AddToZipCommandT = AddToZipCommand_base<D, I...>;
}

#endif
