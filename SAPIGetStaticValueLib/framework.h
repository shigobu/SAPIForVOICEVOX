#pragma once

#define WIN32_LEAN_AND_MEAN             // Windows ヘッダーからほとんど使用されていない部分を除外する
// Windows ヘッダー ファイル
#include <windows.h>

GUID __stdcall GetSPDFIDWaveFormatEx();
GUID __stdcall GetSPDFIDText();
