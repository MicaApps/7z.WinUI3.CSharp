#pragma once

#include "pch.h"
#include <filesystem>
#include <shobjidl_core.h>
#include <shlwapi.h>
#include <sstream>
#include "BaseExplorerCommand.g.h"
#include "SevenZipCommand.g.h"
#include "ExtractToCommand.g.h"
#include "AddTo7zCommand.g.h"
#include "AddToZipCommand.g.h"
#include "CompressAndEmailCommand.g.h"

namespace winrt::ZipShellExt::implementation
{
	struct SubMenu : winrt::implements<SubMenu, IEnumExplorerCommand>
	{
	public:
		SubMenu(winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> commands) : mCommands(std::move(commands)) {};
		IFACEMETHODIMP Next(ULONG celt, __out_ecount_part(celt, *pceltFetched) IExplorerCommand** apUICommand, __out_opt ULONG* pceltFetched);
		IFACEMETHODIMP Skip(ULONG /*celt*/);
		IFACEMETHODIMP Reset();
		IFACEMETHODIMP Clone(__deref_out IEnumExplorerCommand** ppenum);

	private:
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> mCommands;
		uint32_t mIndex{};
	};

	struct BaseExplorerCommand : BaseExplorerCommandT<BaseExplorerCommand, IExplorerCommand>
	{
	public:
		BaseExplorerCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip);
		IFACEMETHODIMP GetCanonicalName(_Out_ GUID* guidCommandName);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		IFACEMETHODIMP GetFlags(_Out_ EXPCMDFLAGS* flags);
		IFACEMETHODIMP EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands);
		virtual winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};

	struct SevenZipCommand : SevenZipCommandT<SevenZipCommand, BaseExplorerCommand>
	{
	public:
		SevenZipCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};

	struct ExtractToCommand : ExtractToCommandT<ExtractToCommand, BaseExplorerCommand>
	{
	public:
		ExtractToCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};

	struct AddTo7zCommand : AddTo7zCommandT<AddTo7zCommand, BaseExplorerCommand>
	{
	public:
		AddTo7zCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};

	struct AddToZipCommand : AddToZipCommandT<AddToZipCommand, BaseExplorerCommand>
	{
	public:
		AddToZipCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};

	struct CompressAndEmailCommand : CompressAndEmailCommandT<CompressAndEmailCommand, BaseExplorerCommand>
	{
	public:
		CompressAndEmailCommand();
		IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
		IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
		IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
		IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*);
		winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SubCommands();
	};
}

namespace winrt::ZipShellExt::factory_implementation
{
	struct BaseExplorerCommand : BaseExplorerCommandT<BaseExplorerCommand, winrt::ZipShellExt::implementation::BaseExplorerCommand>
	{
	};
}