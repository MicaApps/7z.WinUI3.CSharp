// WARNING: Please don't edit this file. It was generated by C++/WinRT v2.0.230706.1

void* winrt_make_ZipShellExt_SevenZipCommand()
{
    return winrt::detach_abi(winrt::make<winrt::ZipShellExt::factory_implementation::SevenZipCommand>());
}
WINRT_EXPORT namespace winrt::ZipShellExt
{
    SevenZipCommand::SevenZipCommand() :
        SevenZipCommand(make<ZipShellExt::implementation::SevenZipCommand>())
    {
    }
}