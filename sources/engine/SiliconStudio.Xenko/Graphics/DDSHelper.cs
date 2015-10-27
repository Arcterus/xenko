﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    internal class DDSHelper
    {
        [Flags]
        public enum ConversionFlags
        {
            None = 0x0,
            Expand = 0x1, // Conversion requires expanded pixel size
            NoAlpha = 0x2, // Conversion requires setting alpha to known value
            Swizzle = 0x4, // BGR/RGB order swizzling required
            Pal8 = 0x8, // Has an 8-bit palette
            Format888 = 0x10, // Source is an 8:8:8 (24bpp) format
            Format565 = 0x20, // Source is a 5:6:5 (16bpp) format
            Format5551 = 0x40, // Source is a 5:5:5:1 (16bpp) format
            Format4444 = 0x80, // Source is a 4:4:4:4 (16bpp) format
            Format44 = 0x100, // Source is a 4:4 (8bpp) format
            Format332 = 0x200, // Source is a 3:3:2 (8bpp) format
            Format8332 = 0x400, // Source is a 8:3:3:2 (16bpp) format
            FormatA8P8 = 0x800, // Has an 8-bit palette with an alpha channel
            CopyMemory = 0x1000, // The content of the memory passed to the DDS Loader is copied to another internal buffer.
            DX10 = 0x10000, // Has the 'DX10' extension header
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LegacyMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LegacyMap" /> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="conversionFlags">The conversion flags.</param>
            /// <param name="pixelFormat">The pixel format.</param>
            public LegacyMap(PixelFormat format, ConversionFlags conversionFlags, DDS.DDSPixelFormat pixelFormat)
            {
                Format = format;
                ConversionFlags = conversionFlags;
                PixelFormat = pixelFormat;
            }

            public PixelFormat Format;
            public ConversionFlags ConversionFlags;
            public DDS.DDSPixelFormat PixelFormat;
        };

        private static readonly LegacyMap[] LegacyMaps = new[]
                                                             {
                                                                 new LegacyMap(PixelFormat.BC1_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.DXT1), // D3DFMT_DXT1
                                                                 new LegacyMap(PixelFormat.BC2_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.DXT3), // D3DFMT_DXT3
                                                                 new LegacyMap(PixelFormat.BC3_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.DXT5), // D3DFMT_DXT5

                                                                 new LegacyMap(PixelFormat.BC2_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.DXT2), // D3DFMT_DXT2 (ignore premultiply)
                                                                 new LegacyMap(PixelFormat.BC3_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.DXT4), // D3DFMT_DXT4 (ignore premultiply)

                                                                 new LegacyMap(PixelFormat.BC4_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.BC4_UNorm),
                                                                 new LegacyMap(PixelFormat.BC4_SNorm, ConversionFlags.None, DDS.DDSPixelFormat.BC4_SNorm),
                                                                 new LegacyMap(PixelFormat.BC5_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.BC5_UNorm),
                                                                 new LegacyMap(PixelFormat.BC5_SNorm, ConversionFlags.None, DDS.DDSPixelFormat.BC5_SNorm),

                                                                 new LegacyMap(PixelFormat.BC4_UNorm, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
                                                                 new LegacyMap(PixelFormat.BC5_UNorm, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),

                                                                 new LegacyMap(PixelFormat.R8G8_B8G8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.R8G8_B8G8), // D3DFMT_R8G8_B8G8
                                                                 new LegacyMap(PixelFormat.G8R8_G8B8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.G8R8_G8B8), // D3DFMT_G8R8_G8B8

                                                                 new LegacyMap(PixelFormat.B8G8R8A8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.A8R8G8B8), // D3DFMT_A8R8G8B8 (uses DXGI 1.1 format)
                                                                 new LegacyMap(PixelFormat.B8G8R8X8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.X8R8G8B8), // D3DFMT_X8R8G8B8 (uses DXGI 1.1 format)
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.A8B8G8R8), // D3DFMT_A8B8G8R8
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.NoAlpha, DDS.DDSPixelFormat.X8B8G8R8), // D3DFMT_X8B8G8R8
                                                                 new LegacyMap(PixelFormat.R16G16_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.G16R16), // D3DFMT_G16R16

                                                                 new LegacyMap(PixelFormat.R10G10B10A2_UNorm, ConversionFlags.Swizzle, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),
                                                                 // D3DFMT_A2R10G10B10 (D3DX reversal issue workaround)
                                                                 new LegacyMap(PixelFormat.R10G10B10A2_UNorm, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),
                                                                 // D3DFMT_A2B10G10R10 (D3DX reversal issue workaround)

                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.NoAlpha
                                                                                                           | ConversionFlags.Format888, DDS.DDSPixelFormat.R8G8B8), // D3DFMT_R8G8B8

                                                                 new LegacyMap(PixelFormat.B5G6R5_UNorm, ConversionFlags.Format565, DDS.DDSPixelFormat.R5G6B5), // D3DFMT_R5G6B5
                                                                 new LegacyMap(PixelFormat.B5G5R5A1_UNorm, ConversionFlags.Format5551, DDS.DDSPixelFormat.A1R5G5B5), // D3DFMT_A1R5G5B5
                                                                 new LegacyMap(PixelFormat.B5G5R5A1_UNorm, ConversionFlags.Format5551
                                                                                                           | ConversionFlags.NoAlpha, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)), // D3DFMT_X1R5G5B5
     
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.Format8332, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)),
                                                                 // D3DFMT_A8R3G3B2
                                                                 new LegacyMap(PixelFormat.B5G6R5_UNorm, ConversionFlags.Expand
                                                                                                         | ConversionFlags.Format332, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 8, 0xe0, 0x1c, 0x03, 0x00)), // D3DFMT_R3G3B2
  
                                                                 new LegacyMap(PixelFormat.R8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.L8), // D3DFMT_L8
                                                                 new LegacyMap(PixelFormat.R16_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.L16), // D3DFMT_L16
                                                                 new LegacyMap(PixelFormat.R8G8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.A8L8), // D3DFMT_A8L8

                                                                 new LegacyMap(PixelFormat.A8_UNorm, ConversionFlags.None, DDS.DDSPixelFormat.A8), // D3DFMT_A8

                                                                 new LegacyMap(PixelFormat.R16G16B16A16_UNorm, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16
                                                                 new LegacyMap(PixelFormat.R16G16B16A16_SNorm, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)), // D3DFMT_Q16W16V16U16
                                                                 new LegacyMap(PixelFormat.R16_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)), // D3DFMT_R16F
                                                                 new LegacyMap(PixelFormat.R16G16_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)), // D3DFMT_G16R16F
                                                                 new LegacyMap(PixelFormat.R16G16B16A16_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16F
                                                                 new LegacyMap(PixelFormat.R32_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)), // D3DFMT_R32F
                                                                 new LegacyMap(PixelFormat.R32G32_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)), // D3DFMT_G32R32F
                                                                 new LegacyMap(PixelFormat.R32G32B32A32_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)), // D3DFMT_A32B32G32R32F

                                                                 new LegacyMap(PixelFormat.R32_Float, ConversionFlags.None, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
                                                                 // D3DFMT_R32F (D3DX uses FourCC 114 instead)

                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.Pal8
                                                                                                           | ConversionFlags.FormatA8P8, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Pal8, 0, 16, 0, 0, 0, 0)), // D3DFMT_A8P8
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.Pal8, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Pal8, 0, 8, 0, 0, 0, 0)), // D3DFMT_P8
#if DIRECTX11_1
    new LegacyMap( PixelFormat.B4G4R4A4_UNorm,     ConversionFlags.Format4444,        DDS.PixelFormat.A4R4G4B4 ), // D3DFMT_A4R4G4B4 (uses DXGI 1.2 format)
    new LegacyMap( PixelFormat.B4G4R4A4_UNorm,     ConversionFlags.NoAlpha
                                      | ConversionFlags.Format4444,      new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb,       0, 16, 0x0f00,     0x00f0,     0x000f,     0x0000     ) ), // D3DFMT_X4R4G4B4 (uses DXGI 1.2 format)
    new LegacyMap( PixelFormat.B4G4R4A4_UNorm,     ConversionFlags.Expand
                                      | ConversionFlags.Format44,        new DDS.PixelFormat(DDS.PixelFormatFlags.Luminance, 0,  8, 0x0f,       0x00,       0x00,       0xf0       ) ), // D3DFMT_A4L4 (uses DXGI 1.2 format)
#else
                                                                 // !DXGI_1_2_FORMATS
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.Format4444, DDS.DDSPixelFormat.A4R4G4B4), // D3DFMT_A4R4G4B4
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.NoAlpha
                                                                                                           | ConversionFlags.Format4444, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),
                                                                 // D3DFMT_X4R4G4B4
                                                                 new LegacyMap(PixelFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                                                                           | ConversionFlags.Format44, new DDS.DDSPixelFormat(DDS.PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)), // D3DFMT_A4L4
#endif
                                                             };


        // Note that many common DDS reader/writers (including D3DX) swap the
        // the RED/BLUE masks for 10:10:10:2 formats. We assumme
        // below that the 'backwards' header mask is being used since it is most
        // likely written by D3DX. The more robust solution is to use the 'DX10'
        // header extension and specify the PixelFormat.R10G10B10A2_UNorm format directly

        // We do not support the following legacy Direct3D 9 formats:
        //      BumpDuDv D3DFMT_V8U8, D3DFMT_Q8W8V8U8, D3DFMT_V16U16, D3DFMT_A2W10V10U10
        //      BumpLuminance D3DFMT_L6V5U5, D3DFMT_X8L8V8U8
        //      FourCC "UYVY" D3DFMT_UYVY
        //      FourCC "YUY2" D3DFMT_YUY2
        //      FourCC 117 D3DFMT_CxV8U8
        //      ZBuffer D3DFMT_D16_LOCKABLE
        //      FourCC 82 D3DFMT_D32F_LOCKABLE
        private static PixelFormat GetDXGIFormat(ref DDS.DDSPixelFormat pixelFormat, DDSFlags flags, out ConversionFlags conversionFlags)
        {
            conversionFlags = ConversionFlags.None;

            int index = 0;
            for (index = 0; index < LegacyMaps.Length; ++index)
            {
                var entry = LegacyMaps[index];

                if ((pixelFormat.Flags & entry.PixelFormat.Flags) != 0)
                {
                    if ((entry.PixelFormat.Flags & DDS.PixelFormatFlags.FourCC) != 0)
                    {
                        if (pixelFormat.FourCC == entry.PixelFormat.FourCC)
                            break;
                    }
                    else if ((entry.PixelFormat.Flags & DDS.PixelFormatFlags.Pal8) != 0)
                    {
                        if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                            break;
                    }
                    else if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                    {
                        // RGB, RGBA, ALPHA, LUMINANCE
                        if (pixelFormat.RBitMask == entry.PixelFormat.RBitMask
                            && pixelFormat.GBitMask == entry.PixelFormat.GBitMask
                            && pixelFormat.BBitMask == entry.PixelFormat.BBitMask
                            && pixelFormat.ABitMask == entry.PixelFormat.ABitMask)
                            break;
                    }
                }
            }

            if (index >= LegacyMaps.Length)
                return PixelFormat.None;

            conversionFlags = LegacyMaps[index].ConversionFlags;
            var format = LegacyMaps[index].Format;

            if ((conversionFlags & ConversionFlags.Expand) != 0 && (flags & DDSFlags.NoLegacyExpansion) != 0)
                return PixelFormat.None;

            if ((format == PixelFormat.R10G10B10A2_UNorm) && (flags & DDSFlags.NoR10B10G10A2Fixup) != 0)
            {
                conversionFlags ^= ConversionFlags.Swizzle;
            }

            return format;
        }


        /// <summary>
        /// Decodes DDS header including optional DX10 extended header
        /// </summary>
        /// <param name="headerPtr">Pointer to the DDS header.</param>
        /// <param name="size">Size of the DDS content.</param>
        /// <param name="flags">Flags used for decoding the DDS header.</param>
        /// <param name="description">Output texture description.</param>
        /// <param name="convFlags">Output conversion flags.</param>
        /// <exception cref="ArgumentException">If the argument headerPtr is null</exception>
        /// <exception cref="InvalidOperationException">If the DDS header contains invalid datas.</exception>
        /// <returns>True if the decoding is successfull, false if this is not a DDS header.</returns>
        private static unsafe bool DecodeDDSHeader(IntPtr headerPtr, int size, DDSFlags flags, out ImageDescription description, out ConversionFlags convFlags)
        {
            description = new ImageDescription();
            convFlags = ConversionFlags.None;

            if (headerPtr == IntPtr.Zero)
                throw new ArgumentException("Pointer to DDS header cannot be null", "headerPtr");

            if (size < (Utilities.SizeOf<DDS.Header>() + sizeof (uint)))
                return false;

            // DDS files always start with the same magic number ("DDS ")
            if (*(uint*) (headerPtr) != DDS.MagicHeader)
                return false;

            var header = *(DDS.Header*) ((byte*) headerPtr + sizeof (int));

            // Verify header to validate DDS file
            if (header.Size != Utilities.SizeOf<DDS.Header>() || header.PixelFormat.Size != Utilities.SizeOf<DDS.DDSPixelFormat>())
                return false;

            // Setup MipLevels
            description.MipLevels = header.MipMapCount;
            if (description.MipLevels == 0)
                description.MipLevels = 1;

            // Check for DX10 extension
            if ((header.PixelFormat.Flags & DDS.PixelFormatFlags.FourCC) != 0 && (new FourCC('D', 'X', '1', '0') == header.PixelFormat.FourCC))
            {
                // Buffer must be big enough for both headers and magic value
                if (size < (Utilities.SizeOf<DDS.Header>() + sizeof (uint) + Utilities.SizeOf<DDS.HeaderDXT10>()))
                    return false;

                var headerDX10 = *(DDS.HeaderDXT10*) ((byte*) headerPtr + sizeof (int) + Utilities.SizeOf<DDS.Header>());
                convFlags |= ConversionFlags.DX10;

                description.ArraySize = headerDX10.ArraySize;
                if (description.ArraySize == 0)
                    throw new InvalidOperationException("Unexpected ArraySize == 0 from DDS HeaderDX10 ");

                description.Format = headerDX10.DXGIFormat;
                if (!description.Format.IsValid())
                    throw new InvalidOperationException("Invalid Format from DDS HeaderDX10 ");

                switch (headerDX10.ResourceDimension)
                {
                    case DDS.ResourceDimension.Texture1D:

                        // D3DX writes 1D textures with a fixed Height of 1
                        if ((header.Flags & DDS.HeaderFlags.Height) != 0 && header.Height != 1)
                            throw new InvalidOperationException("Unexpected Height != 1 from DDS HeaderDX10 ");

                        description.Width = header.Width;
                        description.Height = 1;
                        description.Depth = 1;
                        description.Dimension = TextureDimension.Texture1D;
                        break;

                    case DDS.ResourceDimension.Texture2D:
                        if ((headerDX10.MiscFlags & DDS.ResourceOptionFlags.TextureCube) != 0)
                        {
                            description.ArraySize *= 6;
                            description.Dimension = TextureDimension.TextureCube;
                        }
                        else
                        {
                            description.Dimension = TextureDimension.Texture2D;
                        }

                        description.Width = header.Width;
                        description.Height = header.Height;
                        description.Depth = 1;
                        break;

                    case DDS.ResourceDimension.Texture3D:
                        if ((header.Flags & DDS.HeaderFlags.Volume) == 0)
                            throw new InvalidOperationException("Texture3D missing HeaderFlags.Volume from DDS HeaderDX10");

                        if (description.ArraySize > 1)
                            throw new InvalidOperationException("Unexpected ArraySize > 1 for Texture3D from DDS HeaderDX10");

                        description.Width = header.Width;
                        description.Height = header.Height;
                        description.Depth = header.Depth;
                        description.Dimension = TextureDimension.Texture3D;
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Unexpected dimension [{0}] from DDS HeaderDX10", headerDX10.ResourceDimension));
                }
            }
            else
            {
                description.ArraySize = 1;

                if ((header.Flags & DDS.HeaderFlags.Volume) != 0)
                {
                    description.Width = header.Width;
                    description.Height = header.Height;
                    description.Depth = header.Depth;
                    description.Dimension = TextureDimension.Texture3D;
                }
                else
                {
                    if ((header.CubemapFlags & DDS.CubemapFlags.CubeMap) != 0)
                    {
                        // We require all six faces to be defined
                        if ((header.CubemapFlags & DDS.CubemapFlags.AllFaces) != DDS.CubemapFlags.AllFaces)
                            throw new InvalidOperationException("Unexpected CubeMap, expecting all faces from DDS Header");

                        description.ArraySize = 6;
                        description.Dimension = TextureDimension.TextureCube;
                    }
                    else
                    {
                        description.Dimension = TextureDimension.Texture2D;
                    }

                    description.Width = header.Width;
                    description.Height = header.Height;
                    description.Depth = 1;
                    // Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
                }

                description.Format = GetDXGIFormat(ref header.PixelFormat, flags, out convFlags);

                if (description.Format == PixelFormat.None)
                    throw new InvalidOperationException("Unsupported PixelFormat from DDS Header");
            }

            // Special flag for handling BGR DXGI 1.1 formats
            if ((flags & DDSFlags.ForceRgb) != 0)
            {
                switch ((PixelFormat) description.Format)
                {
                    case PixelFormat.B8G8R8A8_UNorm:
                        description.Format = PixelFormat.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case PixelFormat.B8G8R8X8_UNorm:
                        description.Format = PixelFormat.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;

                    case PixelFormat.B8G8R8A8_Typeless:
                        description.Format = PixelFormat.R8G8B8A8_Typeless;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case PixelFormat.B8G8R8A8_UNorm_SRgb:
                        description.Format = PixelFormat.R8G8B8A8_UNorm_SRgb;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case PixelFormat.B8G8R8X8_Typeless:
                        description.Format = PixelFormat.R8G8B8A8_Typeless;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;

                    case PixelFormat.B8G8R8X8_UNorm_SRgb:
                        description.Format = PixelFormat.R8G8B8A8_UNorm_SRgb;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;
                }
            }

            // Pass DDSFlags copy memory to the conversion flags
            if ((flags & DDSFlags.CopyMemory) != 0)
                convFlags |= ConversionFlags.CopyMemory;

            // Special flag for handling 16bpp formats
            if ((flags & DDSFlags.No16Bpp) != 0)
            {
                switch ((PixelFormat) description.Format)
                {
                    case PixelFormat.B5G6R5_UNorm:
                    case PixelFormat.B5G5R5A1_UNorm:
#if DIRECTX11_1
                    case PixelFormat.B4G4R4A4_UNorm:
#endif
                        description.Format = PixelFormat.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Expand;
                        if (description.Format == PixelFormat.B5G6R5_UNorm)
                            convFlags |= ConversionFlags.NoAlpha;
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Encodes DDS file header (magic value, header, optional DX10 extended header)
        /// </summary>
        /// <param name="flags">Flags used for decoding the DDS header.</param>
        /// <param name="description">Output texture description.</param>
        /// <param name="pDestination">Pointer to the DDS output header. Can be set to IntPtr.Zero to calculated the required bytes.</param>
        /// <param name="maxsize">The maximum size of the destination buffer.</param>
        /// <param name="required">Output the number of bytes required to write the DDS header.</param>
        /// <exception cref="ArgumentException">If the argument headerPtr is null</exception>
        /// <exception cref="InvalidOperationException">If the DDS header contains invalid datas.</exception>
        /// <returns>True if the decoding is successfull, false if this is not a DDS header.</returns>
        private unsafe static void EncodeDDSHeader( ImageDescription description, DDSFlags flags,  IntPtr pDestination, int maxsize, out int required )
        {
            if (description.ArraySize > 1)
            {
                if ((description.ArraySize != 6) || (description.Dimension != TextureDimension.Texture2D) || (description.Dimension != TextureDimension.TextureCube))
                {
                    flags |= DDSFlags.ForceDX10Ext;
                }
            }

            var ddpf = default(DDS.DDSPixelFormat);
            if ((flags & DDSFlags.ForceDX10Ext) == 0)
            {
                switch (description.Format)
                {
                    case PixelFormat.R8G8B8A8_UNorm:
                        ddpf = DDS.DDSPixelFormat.A8B8G8R8;
                        break;
                    case PixelFormat.R16G16_UNorm:
                        ddpf = DDS.DDSPixelFormat.G16R16;
                        break;
                    case PixelFormat.R8G8_UNorm:
                        ddpf = DDS.DDSPixelFormat.A8L8;
                        break;
                    case PixelFormat.R16_UNorm:
                        ddpf = DDS.DDSPixelFormat.L16;
                        break;
                    case PixelFormat.R8_UNorm:
                        ddpf = DDS.DDSPixelFormat.L8;
                        break;
                    case PixelFormat.A8_UNorm:
                        ddpf = DDS.DDSPixelFormat.A8;
                        break;
                    case PixelFormat.R8G8_B8G8_UNorm:
                        ddpf = DDS.DDSPixelFormat.R8G8_B8G8;
                        break;
                    case PixelFormat.G8R8_G8B8_UNorm:
                        ddpf = DDS.DDSPixelFormat.G8R8_G8B8;
                        break;
                    case PixelFormat.BC1_UNorm:
                        ddpf = DDS.DDSPixelFormat.DXT1;
                        break;
                    case PixelFormat.BC2_UNorm:
                        ddpf = DDS.DDSPixelFormat.DXT3;
                        break;
                    case PixelFormat.BC3_UNorm:
                        ddpf = DDS.DDSPixelFormat.DXT5;
                        break;
                    case PixelFormat.BC4_UNorm:
                        ddpf = DDS.DDSPixelFormat.BC4_UNorm;
                        break;
                    case PixelFormat.BC4_SNorm:
                        ddpf = DDS.DDSPixelFormat.BC4_SNorm;
                        break;
                    case PixelFormat.BC5_UNorm:
                        ddpf = DDS.DDSPixelFormat.BC5_UNorm;
                        break;
                    case PixelFormat.BC5_SNorm:
                        ddpf = DDS.DDSPixelFormat.BC5_SNorm;
                        break;
                    case PixelFormat.B5G6R5_UNorm:
                        ddpf = DDS.DDSPixelFormat.R5G6B5;
                        break;
                    case PixelFormat.B5G5R5A1_UNorm:
                        ddpf = DDS.DDSPixelFormat.A1R5G5B5;
                        break;
                    case PixelFormat.B8G8R8A8_UNorm:
                        ddpf = DDS.DDSPixelFormat.A8R8G8B8;
                        break; // DXGI 1.1
                    case PixelFormat.B8G8R8X8_UNorm:
                        ddpf = DDS.DDSPixelFormat.X8R8G8B8;
                        break; // DXGI 1.1
#if DIRECTX11_1
                    case PixelFormat.B4G4R4A4_UNorm:
                        ddpf = DDS.PixelFormat.A4R4G4B4;
                        break;
#endif
                    // Legacy D3DX formats using D3DFMT enum value as FourCC
                    case PixelFormat.R32G32B32A32_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 116; // D3DFMT_A32B32G32R32F
                        break;
                    case PixelFormat.R16G16B16A16_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 113; // D3DFMT_A16B16G16R16F
                        break;
                    case PixelFormat.R16G16B16A16_UNorm:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 36; // D3DFMT_A16B16G16R16
                        break;
                    case PixelFormat.R16G16B16A16_SNorm:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 110; // D3DFMT_Q16W16V16U16
                        break;
                    case PixelFormat.R32G32_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 115; // D3DFMT_G32R32F
                        break;
                    case PixelFormat.R16G16_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 112; // D3DFMT_G16R16F
                        break;
                    case PixelFormat.R32_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 114; // D3DFMT_R32F
                        break;
                    case PixelFormat.R16_Float:
                        ddpf.Size = Utilities.SizeOf<DDS.DDSPixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 111; // D3DFMT_R16F
                        break;
                }
            }

            required = sizeof (int) + Utilities.SizeOf<DDS.Header>();

            if (ddpf.Size == 0)
                required += Utilities.SizeOf<DDS.HeaderDXT10>();

            if (pDestination == IntPtr.Zero)
                return;

            if (maxsize < required)
                throw new ArgumentException("Not enough size for destination buffer", "maxsize");

            *(uint*)(pDestination) = DDS.MagicHeader;

            var header = (DDS.Header*)((byte*)(pDestination) + sizeof (int));

            Utilities.ClearMemory((IntPtr)header, 0, Utilities.SizeOf<DDS.Header>());
            header->Size = Utilities.SizeOf<DDS.Header>();
            header->Flags = DDS.HeaderFlags.Texture;
            header->SurfaceFlags = DDS.SurfaceFlags.Texture;

            if (description.MipLevels > 0)
            {
                header->Flags |= DDS.HeaderFlags.Mipmap;
                header->MipMapCount = description.MipLevels;

                if (header->MipMapCount > 1)
                    header->SurfaceFlags |= DDS.SurfaceFlags.Mipmap;
            }

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    header->Height = description.Height;
                    header->Width = header->Depth = 1;
                    break;

                case TextureDimension.Texture2D:
                case TextureDimension.TextureCube:
                    header->Height = description.Height;
                    header->Width = description.Width;
                    header->Depth = 1;

                    if (description.Dimension == TextureDimension.TextureCube)
                    {
                        header->SurfaceFlags |= DDS.SurfaceFlags.Cubemap;
                        header->CubemapFlags |= DDS.CubemapFlags.AllFaces;
                    }
                    break;

                case TextureDimension.Texture3D:

                    header->Flags |= DDS.HeaderFlags.Volume;
                    header->CubemapFlags |= DDS.CubemapFlags.Volume;
                    header->Height = description.Height;
                    header->Width = description.Width;
                    header->Depth = description.Depth;
                    break;
            }

            int rowPitch, slicePitch;
            int newWidth;
            int newHeight;
            Image.ComputePitch(description.Format, description.Width, description.Height, out rowPitch, out slicePitch, out newWidth, out newHeight);

            if (description.Format.IsCompressed())
            {
                header->Flags |= DDS.HeaderFlags.LinearSize;
                header->PitchOrLinearSize = slicePitch;
            }
            else
            {
                header->Flags |= DDS.HeaderFlags.Pitch;
                header->PitchOrLinearSize = rowPitch;
            }

            if (ddpf.Size == 0)
            {
                header->PixelFormat = DDS.DDSPixelFormat.DX10;

                var ext = (DDS.HeaderDXT10*)((byte*)(header) + Utilities.SizeOf<DDS.Header>());

                Utilities.ClearMemory((IntPtr) ext, 0, Utilities.SizeOf<DDS.HeaderDXT10>());

                ext->DXGIFormat = description.Format;
                switch (description.Dimension)
                {
                    case TextureDimension.Texture1D:
                        ext->ResourceDimension = DDS.ResourceDimension.Texture1D;
                        break;
                    case TextureDimension.Texture2D:
                    case TextureDimension.TextureCube:
                        ext->ResourceDimension = DDS.ResourceDimension.Texture2D;
                        break;
                    case TextureDimension.Texture3D:
                        ext->ResourceDimension = DDS.ResourceDimension.Texture3D;
                        break;

                }

                if (description.Dimension == TextureDimension.TextureCube)
                {
                    ext->MiscFlags |= DDS.ResourceOptionFlags.TextureCube;
                    ext->ArraySize = description.ArraySize / 6;
                }
                else
                {
                    ext->ArraySize = description.ArraySize;
                }
            }
            else
            {
                header->PixelFormat = ddpf;
            }
        }

        enum TEXP_LEGACY_FORMAT
        {
            UNKNOWN = 0,
            R8G8B8,
            R3G3B2,
            A8R3G3B2,
            P8,
            A8P8,
            A4L4,
            B4G4R4A4,
        };

        private static TEXP_LEGACY_FORMAT FindLegacyFormat(ConversionFlags flags)
        {
            var lformat = TEXP_LEGACY_FORMAT.UNKNOWN;

            if ((flags & ConversionFlags.Pal8) != 0)
            {
                lformat = (flags & ConversionFlags.FormatA8P8) != 0 ? TEXP_LEGACY_FORMAT.A8P8 : TEXP_LEGACY_FORMAT.P8;
            }
            else if ((flags & ConversionFlags.Format888) != 0)
                lformat = TEXP_LEGACY_FORMAT.R8G8B8;
            else if ((flags & ConversionFlags.Format332) != 0)
                lformat = TEXP_LEGACY_FORMAT.R3G3B2;
            else if ((flags & ConversionFlags.Format8332) != 0)
                lformat = TEXP_LEGACY_FORMAT.A8R3G3B2;
            else if ((flags & ConversionFlags.Format44) != 0)
                lformat = TEXP_LEGACY_FORMAT.A4L4;
#if DIRECTX11_1
            else if ((flags & ConversionFlags.Format4444) != 0)
                lformat = TEXP_LEGACY_FORMAT.B4G4R4A4;
#endif
            return lformat;
        }

        /// <summary>
        // Converts an image row with optional clearing of alpha value to 1.0
        /// </summary>
        /// <param name="pDestination"></param>
        /// <param name="outSize"></param>
        /// <param name="outFormat"></param>
        /// <param name="pSource"></param>
        /// <param name="inSize"></param>
        /// <param name="inFormat"></param>
        /// <param name="pal8"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        static unsafe bool LegacyExpandScanline( IntPtr pDestination, int outSize, PixelFormat outFormat, 
                                            IntPtr pSource, int inSize, TEXP_LEGACY_FORMAT inFormat,
                                            uint* pal8, ScanlineFlags flags )
        {
            switch (inFormat)
            {
                case TEXP_LEGACY_FORMAT.R8G8B8:
                    if (outFormat != PixelFormat.R8G8B8A8_UNorm)
                        return false;

                    // D3DFMT_R8G8B8 -> PixelFormat.R8G8B8A8_UNorm
                    {
                        var sPtr = (byte*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 3, ocount += 4)
                        {
                            // 24bpp Direct3D 9 files are actually BGR, so need to swizzle as well
                            uint t1 = (uint)(*(sPtr) << 16);
                            uint t2 = (uint)(*(sPtr + 1) << 8);
                            uint t3 = *(sPtr + 2);

                            *(dPtr++) = (t1 | t2 | t3 | 0xff000000);
                            sPtr += 3;
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.R3G3B2:
                    switch (outFormat)
                    {
                        case PixelFormat.R8G8B8A8_UNorm:
                            // D3DFMT_R3G3B2 -> PixelFormat.R8G8B8A8_UNorm
                            {
                                var sPtr = (byte*) (pSource);
                                var dPtr = (uint*) (pDestination);

                                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); ++icount, ocount += 4)
                                {
                                    byte t = *(sPtr++);

                                    uint t1 = (uint)((t & 0xe0) | ((t & 0xe0) >> 3) | ((t & 0xc0) >> 6));
                                    uint t2 = (uint)(((t & 0x1c) << 11) | ((t & 0x1c) << 8) | ((t & 0x18) << 5));
                                    uint t3 = (uint)(((t & 0x03) << 22) | ((t & 0x03) << 20) | ((t & 0x03) << 18) | ((t & 0x03) << 16));

                                    *(dPtr++) = (t1 | t2 | t3 | 0xff000000);
                                }
                            }
                            return true;

                        case PixelFormat.B5G6R5_UNorm:
                            // D3DFMT_R3G3B2 -> PixelFormat.B5G6R5_UNorm
                            {
                                var sPtr = (byte*) (pSource);
                                var dPtr = (short*) (pDestination);

                                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); ++icount, ocount += 2)
                                {
                                    byte t = *(sPtr++);

                                    var t1 = (uint) (((t & 0xe0) << 8) | ((t & 0xc0) << 5));
                                    var t2 = (uint) (((t & 0x1c) << 6) | ((t & 0x1c) << 3));
                                    var t3 = (uint) (((t & 0x03) << 3) | ((t & 0x03) << 1) | ((t & 0x02) >> 1));

                                    *(dPtr++) = (short) (t1 | t2 | t3);
                                }
                            }
                            return true;
                    }
                    break;

                case TEXP_LEGACY_FORMAT.A8R3G3B2:
                    if (outFormat != PixelFormat.R8G8B8A8_UNorm)
                        return false;

                    // D3DFMT_A8R3G3B2 -> PixelFormat.R8G8B8A8_UNorm
                    {
                        var sPtr = (short*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            short t = *(sPtr++);

                            uint t1 = (uint)((t & 0x00e0) | ((t & 0x00e0) >> 3) | ((t & 0x00c0) >> 6));
                            uint t2 = (uint)(((t & 0x001c) << 11) | ((t & 0x001c) << 8) | ((t & 0x0018) << 5));
                            uint t3 = (uint)(((t & 0x0003) << 22) | ((t & 0x0003) << 20) | ((t & 0x0003) << 18) | ((t & 0x0003) << 16));
                            uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint) ((t & 0xff00) << 16));

                            *(dPtr++) = (t1 | t2 | t3 | ta);
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.P8:
                    if ((outFormat != PixelFormat.R8G8B8A8_UNorm) || pal8 == null)
                        return false;

                    // D3DFMT_P8 -> PixelFormat.R8G8B8A8_UNorm
                    {
                        byte* sPtr = (byte*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); ++icount, ocount += 4)
                        {
                            byte t = *(sPtr++);

                            *(dPtr++) = pal8[t];
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.A8P8:
                    if ((outFormat != PixelFormat.R8G8B8A8_UNorm) || pal8 == null)
                        return false;

                    // D3DFMT_A8P8 -> PixelFormat.R8G8B8A8_UNorm
                    {
                        short* sPtr = (short*) (pSource);
                        int* dPtr = (int*) (pDestination);

                        for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            short t = *(sPtr++);

                            uint t1 = pal8[t & 0xff];
                            uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint) ((t & 0xff00) << 16));

                            *(dPtr++) = (int) (t1 | ta);
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.A4L4:
                    switch (outFormat)
                    {
#if DIRECTX11_1
                case PixelFormat.B4G4R4A4_UNorm :
                    // D3DFMT_A4L4 -> PixelFormat.B4G4R4A4_UNorm 
                    {
                        byte * sPtr = (byte*)(pSource);
                        short * dPtr = (short*)(pDestination);

                        for( int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); ++icount, ocount += 2 )
                        {
                            byte t = *(sPtr++);

                            short t1 = (short)(t & 0x0f);
                            ushort ta = (flags & ScanlineFlags.SetAlpha ) != 0 ?  (ushort)0xf000 : (ushort)((t & 0xf0) << 8);

                            *(dPtr++) = (short)(t1 | (t1 << 4) | (t1 << 8) | ta);
                        }
                    }
                    return true;
#endif
                            // DXGI_1_2_FORMATS

                        case PixelFormat.R8G8B8A8_UNorm:
                            // D3DFMT_A4L4 -> PixelFormat.R8G8B8A8_UNorm
                            {
                                byte* sPtr = (byte*) (pSource);
                                var dPtr = (uint*) (pDestination);

                                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); ++icount, ocount += 4)
                                {
                                    byte t = *(sPtr++);

                                    uint t1 = (uint)(((t & 0x0f) << 4) | (t & 0x0f));
                                    uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint) (((t & 0xf0) << 24) | ((t & 0xf0) << 20)));

                                    *(dPtr++) = (t1 | (t1 << 8) | (t1 << 16) | ta);
                                }
                            }
                            return true;
                    }
                    break;

#if !DIRECTX11_1
                case TEXP_LEGACY_FORMAT.B4G4R4A4:
                    if (outFormat != PixelFormat.R8G8B8A8_UNorm)
                        return false;

                    // D3DFMT_A4R4G4B4 -> PixelFormat.R8G8B8A8_UNorm
                    {
                        short* sPtr = (short*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            short t = *(sPtr++);

                            uint t1 = (uint)(((t & 0x0f00) >> 4) | ((t & 0x0f00) >> 8));
                            uint t2 = (uint)(((t & 0x00f0) << 8) | ((t & 0x00f0) << 4));
                            uint t3 = (uint)(((t & 0x000f) << 20) | ((t & 0x000f) << 16));
                            uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint) (((t & 0xf000) << 16) | ((t & 0xf000) << 12)));

                            *(dPtr++) = (t1 | t2 | t3 | ta);
                        }
                    }
                    return true;
#endif
            }
            return false;
        }

        /// <summary>
        /// Load a DDS file in memory
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="size"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public unsafe static Image LoadFromDDSMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            var flags = makeACopy ? DDSFlags.CopyMemory : DDSFlags.None;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            // Directly load image as RGBA instead of BGRA, because OpenGL ES devices don't support it out of the box (extension).
            flags |= DDSFlags.ForceRgb;
#endif

            ConversionFlags convFlags;
            ImageDescription mdata;
            // If the memory pointed is not a DDS memory, return null.
            if (!DecodeDDSHeader(pSource, size, flags, out mdata, out convFlags))
                return null;

            int offset = sizeof (uint) + Utilities.SizeOf<DDS.Header>();
            if ((convFlags & ConversionFlags.DX10) != 0)
                offset += Utilities.SizeOf<DDS.HeaderDXT10>();

            var pal8 = (uint*) 0;
            if ((convFlags & ConversionFlags.Pal8) != 0)
            {
                pal8 = (uint*) ((byte*) (pSource) + offset);
                offset += (256 * sizeof (uint));
            }

            if (size < offset)
                throw new InvalidOperationException();

            var image = CreateImageFromDDS(pSource, offset, size - offset, mdata, (flags & DDSFlags.LegacyDword) != 0 ? Image.PitchFlags.LegacyDword : Image.PitchFlags.None, convFlags, pal8, handle);
            return image;
        }

        public static void SaveToDDSStream(PixelBuffer[] pixelBuffers, int count, ImageDescription description, System.IO.Stream imageStream)
        {
            SaveToDDSStream(pixelBuffers, count, description, DDSFlags.None, imageStream);
        }

        //-------------------------------------------------------------------------------------
        // Save a DDS to a stream
        //-------------------------------------------------------------------------------------
        public unsafe static void SaveToDDSStream(PixelBuffer[] pixelBuffers, int count, ImageDescription metadata, DDSFlags flags, System.IO.Stream stream)
        {
            // Determine memory required
            int totalSize = 0;
            int headerSize = 0;
            EncodeDDSHeader(metadata, flags, IntPtr.Zero, 0, out totalSize);
            headerSize = totalSize;

            int maxSlice = 0;

            for (int i = 0; i < pixelBuffers.Length; ++i)
            {
                int slice = pixelBuffers[i].BufferStride;
                totalSize += slice;
                if (slice > maxSlice)
                    maxSlice = slice;
            }

            Debug.Assert(totalSize > 0);

            // Allocate a single temporary buffer to save the headers and each slice.
            var buffer = new byte[Math.Max(maxSlice, headerSize)];

            fixed (void* pbuffer = buffer)
            {
                int required;
                EncodeDDSHeader(metadata, flags, (IntPtr) pbuffer, headerSize, out required);
                stream.Write(buffer, 0, headerSize);
            }

            int remaining = totalSize - headerSize;
            Debug.Assert(remaining > 0);

            int index = 0;
            for (int item = 0; item < metadata.ArraySize; ++item)
            {
                int d = metadata.Depth;

                for (int level = 0; level < metadata.MipLevels; ++level)
                {
                    for (int slice = 0; slice < d; ++slice)
                    {
                        int pixsize = pixelBuffers[index].BufferStride;
                        Utilities.Read(pixelBuffers[index].DataPointer, buffer, 0, pixsize);
                        stream.Write(buffer, 0, pixsize);
                        ++index;
                    }

                    if (d > 1)
                        d >>= 1;
                }
            }
        }

        /// <summary>
        /// Converts or copies image data from pPixels into scratch image data
        /// </summary>
        /// <param name="pDDS"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="metadata"></param>
        /// <param name="cpFlags"></param>
        /// <param name="convFlags"></param>
        /// <param name="pal8"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static unsafe Image CreateImageFromDDS(IntPtr pDDS, int offset, int size, ImageDescription metadata, Image.PitchFlags cpFlags, ConversionFlags convFlags, uint* pal8, GCHandle? handle)
        {
            if ((convFlags & ConversionFlags.Expand) != 0)
            {
                if ((convFlags & ConversionFlags.Format888) != 0)
                    cpFlags |= Image.PitchFlags.Bpp24;
                else if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444 | ConversionFlags.Format8332 | ConversionFlags.FormatA8P8)) != 0)
                    cpFlags |= Image.PitchFlags.Bpp16;
                else if ((convFlags & (ConversionFlags.Format44 | ConversionFlags.Format332 | ConversionFlags.Pal8)) != 0)
                    cpFlags |= Image.PitchFlags.Bpp8;
            }

            // If source image == dest image and no swizzle/alpha is required, we can return it as-is
            var isCopyNeeded = (convFlags & (ConversionFlags.Expand | ConversionFlags.CopyMemory)) != 0 || ((cpFlags & Image.PitchFlags.LegacyDword) != 0);

            var image = new Image(metadata, pDDS, offset, handle, !isCopyNeeded, cpFlags);

            // Ensure there is enough data to copy from.
            Debug.Assert(size >= image.TotalSizeInBytes);

            if (!isCopyNeeded && (convFlags & (ConversionFlags.Swizzle | ConversionFlags.NoAlpha)) == 0)
                return image;

            var imageDst = isCopyNeeded ? new Image(metadata, IntPtr.Zero, 0, null, false) : image;

            var images = image.PixelBuffer;
            var imagesDst = imageDst.PixelBuffer;

            ScanlineFlags tflags = (convFlags & ConversionFlags.NoAlpha) != 0 ? ScanlineFlags.SetAlpha : ScanlineFlags.None;
            if ((convFlags & ConversionFlags.Swizzle) != 0)
                tflags |= ScanlineFlags.Legacy;

            int index = 0;

            int checkSize = size;

            for (int arrayIndex = 0; arrayIndex < metadata.ArraySize; arrayIndex++)
            {
                int d = metadata.Depth;
                // Else we need to go through each mips/depth slice to convert all scanlines.
                for (int level = 0; level < metadata.MipLevels; ++level)
                {
                    for (int slice = 0; slice < d; ++slice, ++index)
                    {
                        IntPtr pSrc = images[index].DataPointer;
                        IntPtr pDest = imagesDst[index].DataPointer;
                        checkSize -= images[index].BufferStride;
                        if (checkSize < 0)
                            throw new InvalidOperationException("Unexpected end of buffer");

                        if (metadata.Format.IsCompressed())
                        {
                            Utilities.CopyMemory(pDest, pSrc, Math.Min(images[index].BufferStride, imagesDst[index].BufferStride));
                        }
                        else
                        {
                            int spitch = images[index].RowStride;
                            int dpitch = imagesDst[index].RowStride;

                            for (int h = 0; h < images[index].Height; ++h)
                            {
                                if ((convFlags & ConversionFlags.Expand) != 0)
                                {
#if DIRECTX11_1
                                if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444)) != 0)
#else
                                    if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551)) != 0)
#endif
                                    {
                                        ExpandScanline(pDest, dpitch, pSrc, spitch, (convFlags & ConversionFlags.Format565) != 0 ? PixelFormat.B5G6R5_UNorm : PixelFormat.B5G5R5A1_UNorm, tflags);
                                    }
                                    else
                                    {
                                        var lformat = FindLegacyFormat(convFlags);
                                        LegacyExpandScanline(pDest, dpitch, metadata.Format, pSrc, spitch, lformat, pal8, tflags);
                                    }
                                }
                                else if ((convFlags & ConversionFlags.Swizzle) != 0)
                                {
                                    SwizzleScanline(pDest, dpitch, pSrc, spitch, metadata.Format, tflags);
                                }
                                else
                                {
                                    if (pSrc != pDest)
                                        CopyScanline(pDest, dpitch, pSrc, spitch, metadata.Format, tflags);
                                }

                                pSrc = (IntPtr) ((byte*) pSrc + spitch);
                                pDest = (IntPtr) ((byte*) pDest + dpitch);
                            }
                        }
                    }

                    if (d > 1)
                        d >>= 1;
                }
            }

            // Return the imageDst or the original image
            if (isCopyNeeded)
            {
                image.Dispose();
                image = imageDst;
            }
            return image;
        }


        [Flags]
        internal enum ScanlineFlags
        {
            None = 0,
            SetAlpha = 0x1, // Set alpha channel to known opaque value
            Legacy = 0x2, // Enables specific legacy format conversion cases
        };

        /// <summary>
        /// Converts an image row with optional clearing of alpha value to 1.0
        /// </summary>
        /// <param name="pDestination"></param>
        /// <param name="outSize"></param>
        /// <param name="pSource"></param>
        /// <param name="inSize"></param>
        /// <param name="inFormat"></param>
        /// <param name="flags"></param>
        private unsafe static void ExpandScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, PixelFormat inFormat, ScanlineFlags flags)
        {
            switch (inFormat)
            {
                case PixelFormat.B5G6R5_UNorm:
                    // PixelFormat.B5G6R5_UNorm -> PixelFormat.R8G8B8A8_UNorm
                    {
                        var sPtr = (ushort*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (uint ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint) (((t & 0xf800) >> 8) | ((t & 0xe000) >> 13));
                            uint t2 = (uint) (((t & 0x07e0) << 5) | ((t & 0x0600) >> 5));
                            uint t3 = (uint) (((t & 0x001f) << 19) | ((t & 0x001c) << 14));

                            *(dPtr++) = t1 | t2 | t3 | 0xff000000;
                        }
                    }
                    break;

                case PixelFormat.B5G5R5A1_UNorm:
                    // PixelFormat.B5G5R5A1_UNorm -> PixelFormat.R8G8B8A8_UNorm
                    {
                        var sPtr = (ushort*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (uint ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint) (((t & 0x7c00) >> 7) | ((t & 0x7000) >> 12));
                            uint t2 = (uint) (((t & 0x03e0) << 6) | ((t & 0x0380) << 1));
                            uint t3 = (uint) (((t & 0x001f) << 19) | ((t & 0x001c) << 14));
                            uint ta = (uint) ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (((t & 0x8000) != 0 ? 0xff000000 : 0)));

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    break;

#if DIRECTX11_1
                case PixelFormat.B4G4R4A4_UNorm:
                    // PixelFormat.B4G4R4A4_UNorm -> PixelFormat.R8G8B8A8_UNorm
                    {
                        var sPtr = (ushort*) (pSource);
                        var dPtr = (uint*) (pDestination);

                        for (uint ocount = 0, icount = 0; ((icount < inSize) && (ocount < outSize)); icount += 2, ocount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint) (((t & 0x0f00) >> 4) | ((t & 0x0f00) >> 8));
                            uint t2 = (uint) (((t & 0x00f0) << 8) | ((t & 0x00f0) << 4));
                            uint t3 = (uint) (((t & 0x000f) << 20) | ((t & 0x000f) << 16));
                            uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint) ((((t & 0xf000) << 16) | ((t & 0xf000) << 12)));

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    break;
#endif
// DXGI_1_2_FORMATS
            }
        }


        /// <summary>
        /// Copies an image row with optional clearing of alpha value to 1.0.
        /// </summary>
        /// <remarks>
        /// This method can be used in place as well, otherwise copies the image row unmodified.
        /// </remarks>
        /// <param name="pDestination">The destination buffer.</param>
        /// <param name="outSize">The destination size.</param>
        /// <param name="pSource">The source buffer.</param>
        /// <param name="inSize">The source size.</param>
        /// <param name="format">The <see cref="Format"/> of the source scanline.</param>
        /// <param name="flags">Scanline flags used when copying the scanline.</param>
        internal static unsafe void CopyScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, PixelFormat format, ScanlineFlags flags)
        {
            if ((flags & ScanlineFlags.SetAlpha) != 0)
            {
                switch (format)
                {
                    //-----------------------------------------------------------------------------
                    case PixelFormat.R32G32B32A32_Typeless:
                    case PixelFormat.R32G32B32A32_Float:
                    case PixelFormat.R32G32B32A32_UInt:
                    case PixelFormat.R32G32B32A32_SInt:
                        {
                            uint alpha;
                            if (format == PixelFormat.R32G32B32A32_Float)
                                alpha = 0x3f800000;
                            else if (format == PixelFormat.R32G32B32A32_SInt)
                                alpha = 0x7fffffff;
                            else
                                alpha = 0xffffffff;

                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)(pDestination);
                                for (int count = 0; count < outSize; count += 16)
                                {
                                    dPtr += 3;
                                    *(dPtr++) = alpha;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)(pSource);
                                var dPtr = (uint*)(pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 16)
                                {
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = alpha;
                                    sPtr++;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case PixelFormat.R16G16B16A16_Typeless:
                    case PixelFormat.R16G16B16A16_Float:
                    case PixelFormat.R16G16B16A16_UNorm:
                    case PixelFormat.R16G16B16A16_UInt:
                    case PixelFormat.R16G16B16A16_SNorm:
                    case PixelFormat.R16G16B16A16_SInt:
                        {
                            ushort alpha;
                            if (format == PixelFormat.R16G16B16A16_Float)
                                alpha = 0x3c00;
                            else if (format == PixelFormat.R16G16B16A16_SNorm || format == PixelFormat.R16G16B16A16_SInt)
                                alpha = 0x7fff;
                            else
                                alpha = 0xffff;

                            if (pDestination == pSource)
                            {
                                var dPtr = (ushort*)(pDestination);
                                for (int count = 0; count < outSize; count += 8)
                                {
                                    dPtr += 3;
                                    *(dPtr++) = alpha;
                                }
                            }
                            else
                            {
                                var sPtr = (ushort*)(pSource);
                                var dPtr = (ushort*)(pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 8)
                                {
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = *(sPtr++);
                                    *(dPtr++) = alpha;
                                    sPtr++;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case PixelFormat.R10G10B10A2_Typeless:
                    case PixelFormat.R10G10B10A2_UNorm:
                    case PixelFormat.R10G10B10A2_UInt:
                    case PixelFormat.R10G10B10_Xr_Bias_A2_UNorm:
                        {
                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)(pDestination);
                                for (int count = 0; count < outSize; count += 4)
                                {
                                    *dPtr |= 0xC0000000;
                                    ++dPtr;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)(pSource);
                                var dPtr = (uint*)(pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 4)
                                {
                                    *(dPtr++) = *(sPtr++) | 0xC0000000;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case PixelFormat.R8G8B8A8_Typeless:
                    case PixelFormat.R8G8B8A8_UNorm:
                    case PixelFormat.R8G8B8A8_UNorm_SRgb:
                    case PixelFormat.R8G8B8A8_UInt:
                    case PixelFormat.R8G8B8A8_SNorm:
                    case PixelFormat.R8G8B8A8_SInt:
                    case PixelFormat.B8G8R8A8_UNorm:
                    case PixelFormat.B8G8R8A8_Typeless:
                    case PixelFormat.B8G8R8A8_UNorm_SRgb:
                        {
                            uint alpha = (format == PixelFormat.R8G8B8A8_SNorm || format == PixelFormat.R8G8B8A8_SInt) ? 0x7f000000 : 0xff000000;

                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)(pDestination);
                                for (int count = 0; count < outSize; count += 4)
                                {
                                    uint t = *dPtr & 0xFFFFFF;
                                    t |= alpha;
                                    *(dPtr++) = t;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)(pSource);
                                var dPtr = (uint*)(pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 4)
                                {
                                    uint t = *(sPtr++) & 0xFFFFFF;
                                    t |= alpha;
                                    *(dPtr++) = t;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case PixelFormat.B5G5R5A1_UNorm:
                        {
                            if (pDestination == pSource)
                            {
                                var dPtr = (ushort*)(pDestination);
                                for (int count = 0; count < outSize; count += 2)
                                {
                                    *(dPtr++) |= 0x8000;
                                }
                            }
                            else
                            {
                                var sPtr = (ushort*)(pSource);
                                var dPtr = (ushort*)(pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 2)
                                {
                                    *(dPtr++) = (ushort)(*(sPtr++) | 0x8000);
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case PixelFormat.A8_UNorm:
                        Utilities.ClearMemory(pDestination, 0xff, outSize);
                        return;

#if DIRECTX11_1
                    //-----------------------------------------------------------------------------
                    case PixelFormat.B4G4R4A4_UNorm:
                        {
                            if (pDestination == pSource)
                            {
                                var dPtr = (ushort*) (pDestination);
                                for (int count = 0; count < outSize; count += 2)
                                {
                                    *(dPtr++) |= 0xF000;
                                }
                            }
                            else
                            {
                                var sPtr = (ushort*) (pSource);
                                var dPtr = (ushort*) (pDestination);
                                int size = Math.Min(outSize, inSize);
                                for (int count = 0; count < size; count += 2)
                                {
                                    *(dPtr++) = (ushort) (*(sPtr++) | 0xF000);
                                }
                            }
                        }
                        return;
#endif
                    // DXGI_1_2_FORMATS
                }
            }

            // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
            if (pDestination == pSource)
                return;

            Utilities.CopyMemory(pDestination, pSource, Math.Min(outSize, inSize));
        }

        /// <summary>
        /// Swizzles (RGB &lt;-&gt; BGR) an image row with optional clearing of alpha value to 1.0.
        /// </summary>
        /// <param name="pDestination">The destination buffer.</param>
        /// <param name="outSize">The destination size.</param>
        /// <param name="pSource">The source buffer.</param>
        /// <param name="inSize">The source size.</param>
        /// <param name="format">The <see cref="Format"/> of the source scanline.</param>
        /// <param name="flags">Scanline flags used when copying the scanline.</param>
        /// <remarks>
        /// This method can be used in place as well, otherwise copies the image row unmodified.
        /// </remarks>
        internal static unsafe void SwizzleScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, PixelFormat format, ScanlineFlags flags)
        {
            switch (format)
            {
                //---------------------------------------------------------------------------------
                case PixelFormat.R10G10B10A2_Typeless:
                case PixelFormat.R10G10B10A2_UNorm:
                case PixelFormat.R10G10B10A2_UInt:
                case PixelFormat.R10G10B10_Xr_Bias_A2_UNorm:
                    if ((flags & ScanlineFlags.Legacy) != 0)
                    {
                        // Swap Red (R) and Blue (B) channel (used for D3DFMT_A2R10G10B10 legacy sources)
                        if (pDestination == pSource)
                        {
                            var dPtr = (uint*)(pDestination);
                            for (int count = 0; count < outSize; count += 4)
                            {
                                uint t = *dPtr;

                                uint t1 = (t & 0x3ff00000) >> 20;
                                uint t2 = (t & 0x000003ff) << 20;
                                uint t3 = (t & 0x000ffc00);
                                uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xC0000000 : (t & 0xC0000000);

                                *(dPtr++) = t1 | t2 | t3 | ta;
                            }
                        }
                        else
                        {
                            var sPtr = (uint*)(pSource);
                            var dPtr = (uint*)(pDestination);
                            int size = Math.Min(outSize, inSize);
                            for (int count = 0; count < size; count += 4)
                            {
                                uint t = *(sPtr++);

                                uint t1 = (t & 0x3ff00000) >> 20;
                                uint t2 = (t & 0x000003ff) << 20;
                                uint t3 = (t & 0x000ffc00);
                                uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xC0000000 : (t & 0xC0000000);

                                *(dPtr++) = t1 | t2 | t3 | ta;
                            }
                        }
                        return;
                    }
                    break;

                //---------------------------------------------------------------------------------
                case PixelFormat.R8G8B8A8_Typeless:
                case PixelFormat.R8G8B8A8_UNorm:
                case PixelFormat.R8G8B8A8_UNorm_SRgb:
                case PixelFormat.B8G8R8A8_UNorm:
                case PixelFormat.B8G8R8X8_UNorm:
                case PixelFormat.B8G8R8A8_Typeless:
                case PixelFormat.B8G8R8A8_UNorm_SRgb:
                case PixelFormat.B8G8R8X8_Typeless:
                case PixelFormat.B8G8R8X8_UNorm_SRgb:
                    // Swap Red (R) and Blue (B) channels (used to convert from DXGI 1.1 BGR formats to DXGI 1.0 RGB)
                    if (pDestination == pSource)
                    {
                        var dPtr = (uint*)(pDestination);
                        for (int count = 0; count < outSize; count += 4)
                        {
                            uint t = *dPtr;

                            uint t1 = (t & 0x00ff0000) >> 16;
                            uint t2 = (t & 0x000000ff) << 16;
                            uint t3 = (t & 0x0000ff00);
                            uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (t & 0xFF000000);

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    else
                    {
                        var sPtr = (uint*)(pSource);
                        var dPtr = (uint*)(pDestination);
                        int size = Math.Min(outSize, inSize);
                        for (int count = 0; count < size; count += 4)
                        {
                            uint t = *(sPtr++);

                            uint t1 = (t & 0x00ff0000) >> 16;
                            uint t2 = (t & 0x000000ff) << 16;
                            uint t3 = (t & 0x0000ff00);
                            uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (t & 0xFF000000);

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    return;
            }

            // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
            if (pDestination == pSource)
                return;

            Utilities.CopyMemory(pDestination, pSource, Math.Min(outSize, inSize));
        }
 
    }
}