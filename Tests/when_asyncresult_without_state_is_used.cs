﻿using System;
using System.Reflection;
using System.Threading;
using EffectiveAsyncResult;
using PRI.ProductivityExtensions.ReflectionExtensions;
using Xunit;

namespace Tests
{
	public class when_asyncresult_without_state_is_used : IDisposable
	{
		private AsyncResult<int> asyncResult;
		private int callbackWasInvoked;

		public when_asyncresult_without_state_is_used()
		{
			asyncResult = new AsyncResult<int>(ar => Thread.VolatileWrite(ref callbackWasInvoked, 1), null);
		}

		[Fact]
		public void then_newly_created_asyncresult_waithandle_field_is_null()
		{
			var value = asyncResult.GetPrivateFieldValue<WaitHandle>("asyncWaitHandle");
			Assert.Null(value);
		}

		[Fact]
		public void then_has_value_after_completion()
		{
			var expectedResult = 42;
			asyncResult.Complete(true, expectedResult);
			Assert.Equal(expectedResult, asyncResult.End());
		}

		[Fact]
		public void then_completion_cause_calback_invocation()
		{
			asyncResult.Complete(true, 42);
			// give the thread-pool thread time to invoke the callback
			Thread.Sleep(100);
			Assert.Equal(1, callbackWasInvoked);
		}

		[Fact]
		public void then_newly_created_asyncresult_waithandle_property_is_not_set()
		{
			Assert.False(asyncResult.AsyncWaitHandle.WaitOne(0));
		}

		[Fact]
		public void then_completed_asyncresult_waithandle_field_is_null()
		{
			asyncResult.Complete(true);
			var value = asyncResult.GetPrivateFieldValue<WaitHandle>("asyncWaitHandle");
			Assert.Null(value);
		}

		[Fact]
		public void then_completed_asyncresult_waithandle_property_is_set()
		{
			asyncResult.Complete(true);
			Assert.True(asyncResult.AsyncWaitHandle.WaitOne(0));
		}

		[Fact]
		public void then_completed_asyncresult_waithandle_property_is_set_after_waithandle_created()
		{
			// force creation of the WaitHandle;
			var temp = asyncResult.AsyncWaitHandle;
			asyncResult.Complete(true);
			Assert.True(asyncResult.AsyncWaitHandle.WaitOne(0));
		}

		public void Dispose()
		{
			using (asyncResult)
			{
				asyncResult = null;
			}
		}
	}
}
