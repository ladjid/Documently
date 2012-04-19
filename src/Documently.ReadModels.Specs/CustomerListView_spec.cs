using System;
using System.Collections.Generic;
using System.Linq;
using Documently.Messages.CustEvents;
using Documently.ReadModel;
using FakeItEasy;
using MassTransit;
using MassTransit.Context;
using NUnit.Framework;
using Raven.Client.Embedded;

namespace Documently.ReadModels.Specs
{
	class RelocatedImpl
		: Relocated
	{
		public NewId AggregateId { get; set; }
		public uint Version { get; set; }
		public string Street { get; set; }
		public uint StreetNumber { get; set; }
		public string PostalCode { get; set; }
		public string City { get; set; }
	}

	public class when_receiving_out_of_order_messages
	{
		CustomerListView handler;
		Exception caughtException;

		static IConsumeContext<Relocated> context;
			
		[SetUp]
		public void given_handler()
		{
			var store = new EmbeddableDocumentStore
			{
				RunInMemory = true
			};
			store.Initialize();

			handler = new CustomerListView(store);

			when();
		}

		void when()
		{
			var msg = new RelocatedImpl
			{
				AggregateId = NewId.Next(),
				City = "STHLM"
			};

			context = A.Fake<IConsumeContext<Relocated>>();
			A.CallTo(() => context.Message)
				.Returns(msg);

			try
			{
				handler.Consume(context);
			}
			catch (Exception e)
			{
				caughtException = e;
			}
		}

		[Test]
		public void should_not_fail_with_null_ref_ex()
		{
			if (caughtException is NullReferenceException)
				Assert.Fail("Shouldn't have nulled out, but should have called Retry");
		}

		[Test]
		public void should_have_retried_out_of_order_msg()
		{
			A.CallTo(() => context.RetryLater())
				.MustHaveHappened(Repeated.Exactly.Once);
		}
	}
}
