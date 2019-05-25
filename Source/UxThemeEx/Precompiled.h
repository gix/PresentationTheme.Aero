#pragma once
#include <algorithm>
#include <array>
#include <atomic>
#include <cassert>
#include <cstdint>
#include <map>
#include <memory>
#include <mutex>
#include <string>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

// windows.h and ntstatus.h both define common status macros. Prevent the former
// from doing so.
#define WIN32_NO_STATUS
#include <windows.h>
#undef WIN32_NO_STATUS

#include <CommCtrl.h>
#include <CommonControls.h>
#include <ImageHlp.h>
#include <intsafe.h>
#include <memoryapi.h>
#include <objidl.h>
#include <shlwapi.h>
#include <strsafe.h>
#include <tlhelp32.h>
#include <uxtheme.h>
#include <vssym32.h>
#include <wincodec.h>
#include <winnt.h>
#include <winternl.h>
#include <wtypes.h>

#undef WIN32_NO_STATUS
#include <ntstatus.h>
#define WIN32_NO_STATUS
