using HaroohiePals.Nitro.Fs;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.Rom;

record ReadOverlayFilesResult(IReadOnlyList<FatEntry> FatEntries, IReadOnlyList<byte[]> FileData);