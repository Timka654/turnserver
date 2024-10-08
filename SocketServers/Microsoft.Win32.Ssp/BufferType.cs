namespace Microsoft.Win32.Ssp
{
	public enum BufferType
	{
		SECBUFFER_VERSION = 0,
		SECBUFFER_EMPTY = 0,
		SECBUFFER_DATA = 1,
		SECBUFFER_TOKEN = 2,
		SECBUFFER_PKG_PARAMS = 3,
		SECBUFFER_MISSING = 4,
		SECBUFFER_EXTRA = 5,
		SECBUFFER_STREAM_TRAILER = 6,
		SECBUFFER_STREAM_HEADER = 7,
		SECBUFFER_NEGOTIATION_INFO = 8,
		SECBUFFER_PADDING = 9,
		SECBUFFER_STREAM = 10,
		SECBUFFER_MECHLIST = 11,
		SECBUFFER_MECHLIST_SIGNATURE = 12,
		SECBUFFER_TARGET = 13,
		SECBUFFER_CHANNEL_BINDINGS = 14,
		SECBUFFER_CHANGE_PASS_RESPONSE = 0xF
	}
}
