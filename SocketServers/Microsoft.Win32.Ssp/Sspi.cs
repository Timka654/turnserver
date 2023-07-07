using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Ssp
{
	public static class Sspi
	{
		public static bool Succeeded(SecurityStatus result)
		{
			return (int)result >= 0;
		}

		public static bool Failed(SecurityStatus result)
		{
			return (int)result < 0;
		}

		internal static bool Succeeded(int result)
		{
			return result >= 0;
		}

		internal static bool Failed(int result)
		{
			return result < 0;
		}

		public static SecurityStatus EnumerateSecurityPackages(out int packages, out SafeContextBufferHandle secPkgInfos)
		{
			return Convert(Secur32Dll.EnumerateSecurityPackagesA(out packages, out secPkgInfos));
		}

		public unsafe static void AcquireCredentialsHandle(CredentialUse credentialUse, SchannelCred authData, out SafeCredHandle credential, out long expiry)
		{
			GCHandle gCHandle = default(GCHandle);
			IntPtr[] array = null;
			if (authData.cCreds > 0)
			{
				array = new IntPtr[1]
				{
					authData.paCreds1
				};
				gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				authData.paCreds1 = gCHandle.AddrOfPinnedObject();
			}
			try
			{
				CredHandle phCredential;
				int num = Secur32Dll.AcquireCredentialsHandleA(null, "Microsoft Unified Security Protocol Provider", (int)credentialUse, null, &authData, null, null, out phCredential, out expiry);
				if (num != 0)
				{
					throw new SspiException(num, "AcquireCredentialsHandleA");
				}
				credential = new SafeCredHandle(phCredential);
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				if (array != null)
				{
					authData.paCreds1 = array[0];
				}
			}
		}

		public static SafeCredHandle SafeAcquireCredentialsHandle(string package, CredentialUse credentialUse)
		{
			unsafe
			{
				Secur32Dll.AcquireCredentialsHandleA(null, package, (int)credentialUse, null, null, null, null, out CredHandle phCredential, out long _);
				return new SafeCredHandle(phCredential);
			}
		}

		public static SecurityStatus SafeAcceptSecurityContext(ref SafeCredHandle credential, ref SafeCtxtHandle context, ref SecBufferDescEx input, int contextReq, TargetDataRep targetDataRep, ref SafeCtxtHandle newContext, ref SecBufferDescEx output)
		{
			int contextAttr;
			long timeStamp;
			return SafeAcceptSecurityContext(ref credential, ref context, ref input, contextReq, targetDataRep, ref newContext, ref output, out contextAttr, out timeStamp);
		}

		public unsafe static SecurityStatus SafeAcceptSecurityContext(ref SafeCredHandle credential, ref SafeCtxtHandle context, ref SecBufferDescEx input, int contextReq, TargetDataRep targetDataRep, ref SafeCtxtHandle newContext, ref SecBufferDescEx output, out int contextAttr, out long timeStamp)
		{
			try
			{
				input.Pin();
				output.Pin();


                fixed (CtxtHandle* ptr = &context.Handle)
				{
					int error = Secur32Dll.AcceptSecurityContext(ref credential.Handle, (void*)(long)(context.IsInvalid ? ((IntPtr)(void*)null) : ((IntPtr)ptr)), ref input.SecBufferDesc, contextReq, (int)targetDataRep, ref newContext.Handle, ref output.SecBufferDesc, out contextAttr, out timeStamp);
					return Convert(error);
				}
			}
			catch
			{
				contextAttr = 0;
				timeStamp = 0L;
				return SecurityStatus.SEC_E_UNKNOW_ERROR;
			}
			finally
			{
				input.Free();
				output.Free();
			}
		}

		public unsafe static SecurityStatus SafeDecryptMessage(ref SafeCtxtHandle context, ref SecBufferDescEx message, uint MessageSeqNo, void* pfQOP)
		{
			try
			{
				message.Pin();
				int error = Secur32Dll.DecryptMessage(ref context.Handle, ref message.SecBufferDesc, MessageSeqNo, pfQOP);
				return Convert(error);
			}
			catch
			{
				return SecurityStatus.SEC_E_UNKNOW_ERROR;
			}
			finally
			{
				message.Free();
			}
		}

		public unsafe static void EncryptMessage(ref SafeCtxtHandle context, ref SecBufferDescEx message, uint MessageSeqNo, void* pfQOP)
		{
			try
			{
				message.Pin();
				int num = Secur32Dll.EncryptMessage(ref context.Handle, pfQOP, ref message.SecBufferDesc, MessageSeqNo);
				if (num != 0)
				{
					throw new SspiException(num, "EncryptMessage");
				}
			}
			finally
			{
				message.Free();
			}
		}

		public unsafe static void QueryContextAttributes(ref SafeCtxtHandle context, out SecPkgContext_StreamSizes streamSizes)
		{
			fixed (SecPkgContext_StreamSizes* buffer = &streamSizes)
			{
				QueryContextAttributes(ref context, UlAttribute.SECPKG_ATTR_STREAM_SIZES, buffer);
			}
		}

		public unsafe static void QueryContextAttributes(ref SafeCtxtHandle context, UlAttribute attribute, void* buffer)
		{
			int num = Secur32Dll.QueryContextAttributesA(ref context.Handle, (uint)attribute, buffer);
			if (num != 0)
			{
				throw new SspiException(num, "QueryContextAttributesA");
			}
		}

		public unsafe static SecurityStatus SafeQueryContextAttributes(ref SafeCtxtHandle context, out SecPkgContext_StreamSizes streamSizes)
		{
			fixed (SecPkgContext_StreamSizes* buffer = &streamSizes)
			{
				return SafeQueryContextAttributes(ref context, UlAttribute.SECPKG_ATTR_STREAM_SIZES, buffer);
			}
		}

		public unsafe static SecurityStatus SafeQueryContextAttributes(ref SafeCtxtHandle context, out SecPkgContext_Sizes packageSizes)
		{
			fixed (SecPkgContext_Sizes* buffer = &packageSizes)
			{
				return SafeQueryContextAttributes(ref context, UlAttribute.SECPKG_ATTR_SIZES, buffer);
			}
		}

		public unsafe static SecurityStatus SafeQueryContextAttributes(ref SafeCtxtHandle context, out string name)
		{
			SecPkgContext_Names[] array = new SecPkgContext_Names[1];
			fixed (SecPkgContext_Names* buffer = &array[0] )
			{
				SecurityStatus result = SafeQueryContextAttributes(ref context, UlAttribute.SECPKG_ATTR_NAMES, buffer);
				name = Marshal.PtrToStringAnsi(array[0].sUserName);
				Secur32Dll.FreeContextBuffer(array[0].sUserName);
				return result;
			}
		}

		public unsafe static SecurityStatus SafeQueryContextAttributes(ref SafeCtxtHandle context, UlAttribute attribute, void* buffer)
		{
			try
			{
				int error = Secur32Dll.QueryContextAttributesA(ref context.Handle, (uint)attribute, buffer);
				return Convert(error);
			}
			catch
			{
				return SecurityStatus.SEC_E_UNKNOW_ERROR;
			}
		}

		public static SecurityStatus SafeMakeSignature(SafeCtxtHandle context, ref SecBufferDescEx message, int sequence)
		{
			try
			{
				message.Pin();
				int error = Secur32Dll.MakeSignature(ref context.Handle, 0, ref message.SecBufferDesc, sequence);
				return Convert(error);
			}
			catch
			{
				return SecurityStatus.SEC_E_UNKNOW_ERROR;
			}
			finally
			{
				message.Free();
			}
		}

		public static SecurityStatus SafeVerifySignature(SafeCtxtHandle context, ref SecBufferDescEx message, int sequence)
		{
			try
			{
				message.Pin();
				int pfQOP;
				int error = Secur32Dll.VerifySignature(ref context.Handle, ref message.SecBufferDesc, sequence, out pfQOP);
				return Convert(error);
			}
			catch
			{
				return SecurityStatus.SEC_E_UNKNOW_ERROR;
			}
			finally
			{
				message.Free();
			}
		}

		public static SecurityStatus Convert(int error)
		{
			if (Enum.IsDefined(typeof(SecurityStatus), (uint)error))
			{
				return (SecurityStatus)error;
			}
			return SecurityStatus.SEC_E_UNKNOW_ERROR;
		}
	}
}
