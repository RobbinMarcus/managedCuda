﻿//	Copyright (c) 2012, Michael Kunz. All rights reserved.
//	http://kunzmi.github.io/managedcuda
//
//	This file is part of ManagedCuda.
//
//	ManagedCuda is free software: you can redistribute it and/or modify
//	it under the terms of the GNU Lesser General Public License as 
//	published by the Free Software Foundation, either version 2.1 of the 
//	License, or (at your option) any later version.
//
//	ManagedCuda is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//	GNU Lesser General Public License for more details.
//
//	You should have received a copy of the GNU Lesser General Public
//	License along with this library; if not, write to the Free Software
//	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
//	MA 02110-1301  USA, http://www.gnu.org/licenses/.


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ManagedCuda.BasicTypes;

namespace ManagedCuda.NPP
{
	class NPPImage_32fC2 : NPPImageBase
	{
		#region Constructors
		/// <summary>
		/// Allocates new memory on device using NPP-Api.
		/// </summary>
		/// <param name="nWidthPixels">Image width in pixels</param>
		/// <param name="nHeightPixels">Image height in pixels</param>
		public NPPImage_32fC2(int nWidthPixels, int nHeightPixels)
		{
			_sizeOriginal.width = nWidthPixels;
			_sizeOriginal.height = nHeightPixels;
			_sizeRoi.width = nWidthPixels;
			_sizeRoi.height = nHeightPixels;
			_channels = 2;
			_isOwner = true;
			_typeSize = sizeof(float);

			_devPtr = NPPNativeMethods.NPPi.MemAlloc.nppiMalloc_32f_C2(nWidthPixels, nHeightPixels, ref _pitch);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}, Pitch is: {3}, Number of color channels: {4}", DateTime.Now, "nppiMalloc_32f_C2", res, _pitch, _channels));
			
			if (_devPtr.Pointer == 0)
			{
				throw new NPPException("Device allocation error", null);
			}
			_devPtrRoi = _devPtr;
		}

		/// <summary>
		/// Creates a new NPPImage from allocated device ptr.
		/// </summary>
		/// <param name="devPtr">Already allocated device ptr.</param>
		/// <param name="width">Image width in pixels</param>
		/// <param name="height">Image height in pixels</param>
		/// <param name="pitch">Pitch / Line step</param>
		/// <param name="isOwner">If TRUE, devPtr is freed when disposing</param>
		public NPPImage_32fC2(CUdeviceptr devPtr, int width, int height, int pitch, bool isOwner)
		{
			_devPtr = devPtr;
			_devPtrRoi = _devPtr;
			_sizeOriginal.width = width;
			_sizeOriginal.height = height;
			_sizeRoi.width = width;
			_sizeRoi.height = height;
			_pitch = pitch;
			_channels = 2;
			_isOwner = isOwner;
			_typeSize = sizeof(float);
		}

		/// <summary>
		/// Creates a new NPPImage from allocated device ptr. Does not take ownership of decPtr.
		/// </summary>
		/// <param name="devPtr">Already allocated device ptr.</param>
		/// <param name="width">Image width in pixels</param>
		/// <param name="height">Image height in pixels</param>
		/// <param name="pitch">Pitch / Line step</param>
		public NPPImage_32fC2(CUdeviceptr devPtr, int width, int height, int pitch)
			: this(devPtr, width, height, pitch, false)
		{

		}

		/// <summary>
		/// Creates a new NPPImage from allocated device ptr. Does not take ownership of inner image device pointer.
		/// </summary>
		/// <param name="image">NPP image</param>
		public NPPImage_32fC2(NPPImageBase image)
			: this(image.DevicePointer, image.Width, image.Height, image.Pitch, false)
		{

		}

		/// <summary>
		/// Allocates new memory on device using NPP-Api.
		/// </summary>
		/// <param name="size">Image size</param>
		public NPPImage_32fC2(NppiSize size)
			: this(size.width, size.height)
		{ 
			
		}

		/// <summary>
		/// Creates a new NPPImage from allocated device ptr.
		/// </summary>
		/// <param name="devPtr">Already allocated device ptr.</param>
		/// <param name="size">Image size</param>
		/// <param name="pitch">Pitch / Line step</param>
		/// <param name="isOwner">If TRUE, devPtr is freed when disposing</param>
		public NPPImage_32fC2(CUdeviceptr devPtr, NppiSize size, int pitch, bool isOwner)
			: this(devPtr, size.width, size.height, pitch, isOwner)
		{ 
			
		}

		/// <summary>
		/// Creates a new NPPImage from allocated device ptr.
		/// </summary>
		/// <param name="devPtr">Already allocated device ptr.</param>
		/// <param name="size">Image size</param>
		/// <param name="pitch">Pitch / Line step</param>
		public NPPImage_32fC2(CUdeviceptr devPtr, NppiSize size, int pitch)
			: this(devPtr, size.width, size.height, pitch)
		{

		}

		/// <summary>
		/// For dispose
		/// </summary>
		~NPPImage_32fC2()
		{
			Dispose (false);
		}
		#endregion

		#region Converter operators

		/// <summary>
		/// Converts a NPPImage to a CudaPitchedDeviceVariable
		/// </summary>
		public CudaPitchedDeviceVariable<VectorTypes.float2> ToCudaPitchedDeviceVariable()
		{
			return new CudaPitchedDeviceVariable<VectorTypes.float2>(_devPtr, _sizeOriginal.width, _sizeOriginal.height, _pitch);
		}

		/// <summary>
		/// Converts a NPPImage to a CudaPitchedDeviceVariable
		/// </summary>
		/// <param name="img">NPPImage</param>
		/// <returns>CudaPitchedDeviceVariable with the same device pointer and size of NPPImage without ROI information</returns>
		public static implicit operator CudaPitchedDeviceVariable<VectorTypes.float2>(NPPImage_32fC2 img)
		{
			return img.ToCudaPitchedDeviceVariable();
		}

		/// <summary>
		/// Converts a CudaPitchedDeviceVariable to a NPPImage 
		/// </summary>
		/// <param name="img">CudaPitchedDeviceVariable</param>
		/// <returns>NPPImage with the same device pointer and size of CudaPitchedDeviceVariable with ROI set to full image</returns>
		public static implicit operator NPPImage_32fC2(CudaPitchedDeviceVariable<VectorTypes.float2> img)
		{
			return img.ToNPPImage();
		}
		#endregion

		/// <summary>
		/// Image composition using image alpha values (0 - max channel pixel value).
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="dest">Destination image</param>
		/// <param name="nppAlphaOp">alpha compositing operation</param>
		public void AlphaComp(NPPImage_32fC2 src2, NPPImage_32fC2 dest, NppiAlphaOp nppAlphaOp)
		{
			status = NPPNativeMethods.NPPi.AlphaComp.nppiAlphaComp_32f_AC1R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, dest.DevicePointerRoi, dest.Pitch, _sizeRoi, nppAlphaOp);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAlphaComp_32f_AC1R", status));
			NPPException.CheckNppStatus(status, this);
		}
		#region Set
		/// <summary>
		/// Set pixel values to nValue.
		/// </summary>
		/// <param name="nValue">Value to be set</param>
		public void Set(float[] nValue)
		{
			status = NPPNativeMethods.NPPi.MemSet.nppiSet_32f_C2R(nValue, _devPtrRoi, _pitch, _sizeRoi);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiSet_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		#endregion

		#region MaxError
		/// <summary>
		/// image maximum error. User buffer is internally allocated and freed.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		public void MaxError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError)
		{
			int bufferSize = MaxErrorGetBufferHostSize();
			CudaDeviceVariable<byte> buffer = new CudaDeviceVariable<byte>(bufferSize);
			status = NPPNativeMethods.NPPi.MaximumError.nppiMaximumError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiMaximumError_32f_C2R", status));
			buffer.Dispose();
			NPPException.CheckNppStatus(status, this);
		}

		/// <summary>
		/// image maximum error.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		/// <param name="buffer">Pointer to the user-allocated scratch buffer required for the MaxError operation.</param>
		public void MaxError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError, CudaDeviceVariable<byte> buffer)
		{
			int bufferSize = MaxErrorGetBufferHostSize();
			if (bufferSize > buffer.Size) throw new NPPException("Provided buffer is too small.");

			status = NPPNativeMethods.NPPi.MaximumError.nppiMaximumError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiFilterMedian_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		/// <summary>
		/// Device scratch buffer size (in bytes) for MaxError.
		/// </summary>
		/// <returns></returns>
		public int MaxErrorGetBufferHostSize()
		{
			int bufferSize = 0;
			status = NPPNativeMethods.NPPi.MaximumError.nppiMaximumErrorGetBufferHostSize_32f_C2R(_sizeRoi, ref bufferSize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiMaximumErrorGetBufferHostSize_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
			return bufferSize;
		}
		#endregion

		#region AverageError
		/// <summary>
		/// image average error. User buffer is internally allocated and freed.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		public void AverageError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError)
		{
			int bufferSize = AverageErrorGetBufferHostSize();
			CudaDeviceVariable<byte> buffer = new CudaDeviceVariable<byte>(bufferSize);
			status = NPPNativeMethods.NPPi.AverageError.nppiAverageError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageError_32f_C2R", status));
			buffer.Dispose();
			NPPException.CheckNppStatus(status, this);
		}

		/// <summary>
		/// image average error.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		/// <param name="buffer">Pointer to the user-allocated scratch buffer required for the AverageError operation.</param>
		public void AverageError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError, CudaDeviceVariable<byte> buffer)
		{
			int bufferSize = AverageErrorGetBufferHostSize();
			if (bufferSize > buffer.Size) throw new NPPException("Provided buffer is too small.");

			status = NPPNativeMethods.NPPi.AverageError.nppiAverageError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageError_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		/// <summary>
		/// Device scratch buffer size (in bytes) for AverageError.
		/// </summary>
		/// <returns></returns>
		public int AverageErrorGetBufferHostSize()
		{
			int bufferSize = 0;
			status = NPPNativeMethods.NPPi.AverageError.nppiAverageErrorGetBufferHostSize_32f_C2R(_sizeRoi, ref bufferSize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageErrorGetBufferHostSize_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
			return bufferSize;
		}
		#endregion

		#region MaximumRelative_Error
		/// <summary>
		/// image maximum relative error. User buffer is internally allocated and freed.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		public void MaximumRelativeError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError)
		{
			int bufferSize = MaximumRelativeErrorGetBufferHostSize();
			CudaDeviceVariable<byte> buffer = new CudaDeviceVariable<byte>(bufferSize);
			status = NPPNativeMethods.NPPi.MaximumRelativeError.nppiMaximumRelativeError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiMaximumRelativeError_32f_C2R", status));
			buffer.Dispose();
			NPPException.CheckNppStatus(status, this);
		}

		/// <summary>
		/// image maximum relative error.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		/// <param name="buffer">Pointer to the user-allocated scratch buffer required for the MaximumRelativeError operation.</param>
		public void MaximumRelativeError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError, CudaDeviceVariable<byte> buffer)
		{
			int bufferSize = MaximumRelativeErrorGetBufferHostSize();
			if (bufferSize > buffer.Size) throw new NPPException("Provided buffer is too small.");

			status = NPPNativeMethods.NPPi.MaximumRelativeError.nppiMaximumRelativeError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiMaximumRelativeError_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		/// <summary>
		/// Device scratch buffer size (in bytes) for MaximumRelativeError.
		/// </summary>
		/// <returns></returns>
		public int MaximumRelativeErrorGetBufferHostSize()
		{
			int bufferSize = 0;
			status = NPPNativeMethods.NPPi.MaximumRelativeError.nppiMaximumRelativeErrorGetBufferHostSize_32f_C2R(_sizeRoi, ref bufferSize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiMaximumRelativeErrorGetBufferHostSize_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
			return bufferSize;
		}
		#endregion

		#region AverageRelative_Error
		/// <summary>
		/// image average relative error. User buffer is internally allocated and freed.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		public void AverageRelativeError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError)
		{
			int bufferSize = AverageRelativeErrorGetBufferHostSize();
			CudaDeviceVariable<byte> buffer = new CudaDeviceVariable<byte>(bufferSize);
			status = NPPNativeMethods.NPPi.AverageRelativeError.nppiAverageRelativeError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageRelativeError_32f_C2R", status));
			buffer.Dispose();
			NPPException.CheckNppStatus(status, this);
		}

		/// <summary>
		/// image average relative error.
		/// </summary>
		/// <param name="src2">2nd source image</param>
		/// <param name="pError">Pointer to the computed error.</param>
		/// <param name="buffer">Pointer to the user-allocated scratch buffer required for the AverageRelativeError operation.</param>
		public void AverageRelativeError(NPPImage_32fC2 src2, CudaDeviceVariable<double> pError, CudaDeviceVariable<byte> buffer)
		{
			int bufferSize = AverageRelativeErrorGetBufferHostSize();
			if (bufferSize > buffer.Size) throw new NPPException("Provided buffer is too small.");

			status = NPPNativeMethods.NPPi.AverageRelativeError.nppiAverageRelativeError_32f_C2R(_devPtrRoi, _pitch, src2.DevicePointerRoi, src2.Pitch, _sizeRoi, pError.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageRelativeError_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		/// <summary>
		/// Device scratch buffer size (in bytes) for AverageRelativeError.
		/// </summary>
		/// <returns></returns>
		public int AverageRelativeErrorGetBufferHostSize()
		{
			int bufferSize = 0;
			status = NPPNativeMethods.NPPi.AverageRelativeError.nppiAverageRelativeErrorGetBufferHostSize_32f_C2R(_sizeRoi, ref bufferSize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiAverageRelativeErrorGetBufferHostSize_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
			return bufferSize;
		}
		#endregion


		#region ColorTwist
		/// <summary>
		/// An input color twist matrix with floating-point pixel values is applied
		/// within ROI.
		/// </summary>
		/// <param name="dest">Destination image</param>
		/// <param name="twistMatrix">The color twist matrix with floating-point pixel values [3,4].</param>
		public void ColorTwist(NPPImage_32fC2 dest, float[,] twistMatrix)
		{
			status = NPPNativeMethods.NPPi.ColorProcessing.nppiColorTwist_32f_C2R(_devPtr, _pitch, dest.DevicePointer, dest.Pitch, _sizeRoi, twistMatrix);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiColorTwist_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}

		/// <summary>
		/// in place color twist.
		/// 
		/// An input color twist matrix with floating-point coefficient values is applied
		/// within ROI.
		/// </summary>
		/// <param name="aTwist">The color twist matrix with floating-point coefficient values. [3,4]</param>
		public void ColorTwist(float[,] aTwist)
		{
			status = NPPNativeMethods.NPPi.ColorProcessing.nppiColorTwist_32f_C2IR(_devPtr, _pitch, _sizeRoi, aTwist);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiColorTwist_32f_C2IR", status));
			NPPException.CheckNppStatus(status, this);
		}
		#endregion


		#region FilterBorder
		/// <summary>
		/// Two channel 32-bit float convolution filter with border control.<para/>
		/// General purpose 2D convolution filter using floating-point weights with border control.<para/>
		/// Pixels under the mask are multiplied by the respective weights in the mask
		/// and the results are summed. Before writing the result pixel the sum is scaled
		/// back via division by nDivisor. If any portion of the mask overlaps the source
		/// image boundary the requested border type operation is applied to all mask pixels
		/// which fall outside of the source image. <para/>
		/// </summary>
		/// <param name="dest">Destination image</param>
		/// <param name="pKernel">Pointer to the start address of the kernel coefficient array. Coeffcients are expected to be stored in reverse order</param>
		/// <param name="nKernelSize">Width and Height of the rectangular kernel.</param>
		/// <param name="oAnchor">X and Y offsets of the kernel origin frame of reference relative to the source pixel.</param>
		/// <param name="eBorderType">The border type operation to be applied at source image border boundaries.</param>
		public void FilterBorder(NPPImage_32fC2 dest, CudaDeviceVariable<float> pKernel, NppiSize nKernelSize, NppiPoint oAnchor, NppiBorderType eBorderType)
		{
			status = NPPNativeMethods.NPPi.FilterBorder.nppiFilterBorder_32f_C2R(_devPtr, _pitch, _sizeOriginal, _pointRoi, dest.DevicePointerRoi, dest.Pitch, dest.SizeRoi, pKernel.DevicePointer, nKernelSize, oAnchor, eBorderType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiFilterBorder_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		#endregion

		#region Filter

		/// <summary>
		/// convolution filter.
		/// </summary>
		/// <param name="dst">Destination-Image</param>
		/// <param name="pKernel">Pointer to the start address of the kernel coefficient array.<para/>
		/// Coefficients are expected to be stored in reverse order.</param>
		/// <param name="oKernelSize">Width and Height of the rectangular kernel.</param>
		/// <param name="oAnchor">X and Y offsets of the kernel origin frame of reference</param>
		public void Filter(NPPImage_32fC2 dst, CudaDeviceVariable<float> pKernel, NppiSize oKernelSize, NppiPoint oAnchor)
		{
			status = NPPNativeMethods.NPPi.Convolution.nppiFilter_32f_C2R(_devPtrRoi, _pitch, dst.DevicePointerRoi, dst.Pitch, _sizeRoi, pKernel.DevicePointer, oKernelSize, oAnchor);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "nppiFilter_32f_C2R", status));
			NPPException.CheckNppStatus(status, this);
		}
		#endregion
	}
}
