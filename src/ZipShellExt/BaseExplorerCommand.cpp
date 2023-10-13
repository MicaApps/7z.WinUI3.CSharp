#include "pch.h"
#include "BaseExplorerCommand.h"

#include <codecvt>

#include "BaseExplorerCommand.g.cpp"
#include <fstream>

namespace winrt::ZipShellExt::implementation
{
	/// <summary>
	/// IExplorerCommand 接口的详细介绍：https://learn.microsoft.com/zh-cn/windows/win32/api/shobjidl_core/nn-shobjidl_core-iexplorercommand
	/// 添加菜单方法
	/// 1.需要再BaseExplorerCommand.idl中定义菜单项，类似SevenZipCommand
	/// 2.在BaseExplorerCommand.h中添加相应的接口实现，类似SevenZipCommand，使用到哪个方法，在相应的子类中实现哪个方法。比如在子类中实现了GetTitle()方法，就在子类中实现GetTitle()方法
	/// 3.在BaseExplorerCommand.h中添加实现子类生成的文件，比如SevenZipCommand子类对应的文件是SevenZipCommand.g.h，需要包含头文件 "SevenZipCommand.g.h"
	/// 4.在BaseExplorerCommand.cpp中实现对应的方法
	/// 5.如果需要在该菜单中添加二级菜单，则需要实现子类的SubCommands()方法，并为其添加相应的内容，参考SevenZipCommand中的实现
	/// 6.该菜单被用户点击后，需要处理用户点击的内容，用户点击后返回的内容在Invoke()方法中，其中IShellItemArray中包含点击菜单选中的内容
	/// </summary>
	
	/// <summary>
	/// SubMenu 部分
	/// </summary>
	IFACEMETHODIMP SubMenu::Next(ULONG celt, __out_ecount_part(celt, *pceltFetched) IExplorerCommand** apUICommand, __out_opt ULONG* pceltFetched)
	{
		const uint32_t oldIndex = mIndex;
		const uint32_t endIndex = mIndex + celt;
		const uint32_t commandCount = mCommands.Size();
		for (; mIndex < endIndex && mIndex < commandCount; mIndex++)
		{
			mCommands.GetAt(mIndex).try_as<IExplorerCommand>().copy_to(apUICommand);
		}

		const uint32_t fetched = mIndex - oldIndex;
		ULONG outParam = static_cast<ULONG>(fetched);
		if (pceltFetched != nullptr)
		{
			*pceltFetched = outParam;
		}
		return (fetched == celt) ? S_OK : S_FALSE;
	}

	IFACEMETHODIMP SubMenu::Skip(ULONG /*celt*/)
	{
		return E_NOTIMPL;
	}

	IFACEMETHODIMP SubMenu::Reset()
	{
		mIndex = 0;
		return S_OK;
	}

	IFACEMETHODIMP SubMenu::Clone(__deref_out IEnumExplorerCommand** ppenum)
	{
		*ppenum = nullptr; return E_NOTIMPL;
	}

	/// <summary>
	/// BaseExplorerCommand 部分
	/// </summary>
	BaseExplorerCommand::BaseExplorerCommand() {};

	IFACEMETHODIMP BaseExplorerCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP BaseExplorerCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP BaseExplorerCommand::GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip)
	{
		*infoTip = nullptr;
		return E_NOTIMPL;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetCanonicalName(_Out_ GUID* guidCommandName)
	{
		*guidCommandName = GUID_NULL;
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Disabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetFlags(_Out_ EXPCMDFLAGS* flags)
	{
		auto subCommands = SubCommands();
		bool hasSubCommands = subCommands != nullptr && subCommands.Size() > 0;
		*flags = !hasSubCommands ? ECF_DEFAULT : ECF_HASSUBCOMMANDS;
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands)
	{
		*enumCommands = nullptr;
		auto subCommands = SubCommands();
		bool hasSubCommands = subCommands != nullptr && subCommands.Size() > 0;
		if (hasSubCommands)
		{
			auto subMenu = winrt::make<SubMenu>(SubCommands());
			winrt::hresult result = subMenu.as(IID_PPV_ARGS(enumCommands));
			return result;
		}
		return E_NOTIMPL;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> BaseExplorerCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// SevenZipCommand 部分
	/// </summary>
	SevenZipCommand::SevenZipCommand() {};

	IFACEMETHODIMP SevenZipCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		// 应用右键菜单显示的标题
		winrt::hstring title = L"7zip";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP SevenZipCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		// 应用右键菜单显示的图标路径，需要ICO格式，其中Package.Current.InstalledLocation是当前应用的安装目录
		winrt::hstring iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\AppIcon.ico";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP SevenZipCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		// 应用右键菜单的命令项关联的状态信息
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);

		return S_OK;
	}

	IFACEMETHODIMP SevenZipCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		// 用户点击了菜单后，触发的命令。其中IShellItemArray包含点击菜单后选中项目的信息。
		constexpr winrt::guid uuid = winrt::guid_of<SevenZipCommand>();
		
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> SevenZipCommand::SubCommands()
	{
		// 该列表包含该菜单包含的下一级菜单的内容
		return winrt::single_threaded_vector<winrt::ZipShellExt::BaseExplorerCommand>(
			{
				winrt::make<ExtractToCommand>(),
				winrt::make<AddTo7zCommand>(),
				winrt::make<AddToZipCommand>(),
				winrt::make<CompressAndEmailCommand>()
			}).GetView();
	}

	/// <summary>
	/// ExtractToCommand 部分
	/// </summary>
	ExtractToCommand::ExtractToCommand() {};

	IFACEMETHODIMP ExtractToCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"Extract to ***";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP ExtractToCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		DWORD value = 0;
		DWORD size = sizeof(value);
		winrt::hstring iconPath = L"";
		if (const auto result = SHRegGetValueW(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", L"AppsUseLightTheme", SRRF_RT_DWORD, nullptr, &value, &size); result == ERROR_SUCCESS && !!value)
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\unzip Light.ico";
		}
		else
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\unzip Dark.ico";
		}
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP ExtractToCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP ExtractToCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<ExtractToCommand>();
		std::vector<std::wstring> FilePaths;

		//获取选择的所有文件的路径
		//Get paths of all selected files
		if (selection) 
		{
			DWORD Count = 0;
			if (SUCCEEDED(selection->GetCount(&Count)))
			{
				for (DWORD i = 0; i < Count; ++i)
				{
					winrt::com_ptr<IShellItem> Item;
					if (SUCCEEDED(selection->GetItemAt(i, Item.put())))
					{
						LPWSTR DisplayName = nullptr;
						if (SUCCEEDED(Item->GetDisplayName(
							SIGDN_FILESYSPATH,
							&DisplayName)))
						{
							FilePaths.push_back(std::wstring(DisplayName));
							::CoTaskMemFree(DisplayName);
						}
					}
				}
			}
		}


		//打印所有选中文件的路径，请看看怎么才能支持中文
		winrt::hstring tempFilePath = Windows::Storage::ApplicationData::Current().LocalCacheFolder().Path();
		std::string outputFilePath = to_string(tempFilePath) + std::string("\\ExtractPathsTempFile.out");
		std::string tmp;

		std::wstring_convert<std::codecvt_utf8<wchar_t>> conv; //创建utf8转换器

		std::ofstream destFile(outputFilePath, std::ios::out);

		destFile << FilePaths.size() << std::endl; //第一行输出文件数量便于读取

		for (int i = 0;i < FilePaths.size(); ++i)
		{
			tmp = conv.to_bytes(FilePaths[i]);
			destFile << tmp << std::endl;
		}
		//MessageBox(NULL, tmp, NULL, MB_OK);
		destFile.close();



		std::wstring appName = L"";
		std::wstring commandLineStr = appName + L"7Zip.App.exe -extract";
		//这里目前只选取了第一个文件的路径，后期做叠加窗口

		LPWSTR cmdLine = StrDupW(commandLineStr.c_str());

		STARTUPINFO si;
		PROCESS_INFORMATION pi;
		ZeroMemory(&si, sizeof(si));
		ZeroMemory(&pi, sizeof(pi));

		CreateProcessW(NULL, cmdLine, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);
		int lastErr = GetLastError();
		
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> ExtractToCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// AddTo7zCommand 部分
	/// </summary>
	AddTo7zCommand::AddTo7zCommand() {};

	IFACEMETHODIMP AddTo7zCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"Add to ***.7z";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP AddTo7zCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		DWORD value = 0;
		DWORD size = sizeof(value);
		winrt::hstring iconPath = L"";
		if (const auto result = SHRegGetValueW(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", L"AppsUseLightTheme", SRRF_RT_DWORD, nullptr, &value, &size); result == ERROR_SUCCESS && !!value)
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\zip Light.ico";
		}
		else
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\zip Dark.ico";
		}
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP AddTo7zCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP AddTo7zCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<AddTo7zCommand>();
		std::vector<std::wstring> FilePaths;

		if (selection)
		{
			DWORD Count = 0;
			if (SUCCEEDED(selection->GetCount(&Count)))
			{
				for (DWORD i = 0; i < Count; ++i)
				{
					winrt::com_ptr<IShellItem> Item;
					if (SUCCEEDED(selection->GetItemAt(i, Item.put())))
					{
						LPWSTR DisplayName = nullptr;
						if (SUCCEEDED(Item->GetDisplayName(
							SIGDN_FILESYSPATH,
							&DisplayName)))
						{
							FilePaths.push_back(std::wstring(DisplayName));
							::CoTaskMemFree(DisplayName);
						}
					}
				}
			}
		}

		std::wstring appName = L"7Zip.App.exe";
		std::wstring commandLineStr = appName + L" -compress " + L"-7z "+ FilePaths.at(0);
		//这里目前只选取了第一个文件的路径，后期做叠加窗口
		LPWSTR cmdLine = StrDupW(commandLineStr.c_str());

		STARTUPINFO si;
		PROCESS_INFORMATION pi;
		ZeroMemory(&si, sizeof(si));
		ZeroMemory(&pi, sizeof(pi));

		CreateProcessW(NULL, cmdLine, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);
		int lastErr = GetLastError();

		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> AddTo7zCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// AddToZipCommand 部分
	/// </summary>
	AddToZipCommand::AddToZipCommand() {};

	IFACEMETHODIMP AddToZipCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"Add to ***.zip";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP AddToZipCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		DWORD value = 0;
		DWORD size = sizeof(value);
		winrt::hstring iconPath = L"";
		if (const auto result = SHRegGetValueW(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", L"AppsUseLightTheme", SRRF_RT_DWORD, nullptr, &value, &size); result == ERROR_SUCCESS && !!value) 
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\zip Light.ico";
		}
		else 
		{
			iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\zip Dark.ico";
		}
		
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP AddToZipCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP AddToZipCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<AddToZipCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> AddToZipCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// CompressAndEmailCommand 部分
	/// </summary>
	CompressAndEmailCommand::CompressAndEmailCommand() {};

	IFACEMETHODIMP CompressAndEmailCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"Compress and email";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP CompressAndEmailCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP CompressAndEmailCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP CompressAndEmailCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<CompressAndEmailCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::ZipShellExt::BaseExplorerCommand> CompressAndEmailCommand::SubCommands()
	{
		return nullptr;
	}
}