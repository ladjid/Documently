using Documently.Messages;
using Documently.Messages.CustEvents;
using MassTransit;
using Raven.Client;

namespace Documently.ReadModel
{
	public class CustomerListView : HandlesEvent<Registered>, Consumes<Relocated>.Context
	{
		private readonly IDocumentStore _DocumentStore;

		public CustomerListView(IDocumentStore documentStore)
		{
			_DocumentStore = documentStore;
		}

		public void Consume(Relocated evt)
		{
		}

	    public void Consume(IConsumeContext<Relocated> context)
	    {
	        var message = context.Message;
            using (var session = _DocumentStore.OpenSession())
            {
                var dto = session.Load<CustomerListDto>(Dto.GetDtoIdOf<CustomerListDto>(message.AggregateId));
				
				if (dto == null)
				{
					context.RetryLater();
					return;
				}

                dto.City = message.City;
                session.SaveChanges();
            }
	    }

	    public void Consume(Registered evt)
		{
			using (var session = _DocumentStore.OpenSession())
			{
				var dto = new CustomerListDto {AggregateId = evt.AggregateId, City = evt.Address.City, Name = evt.CustomerName};
				session.Store(dto);
				session.SaveChanges();
			}
		}
	}
}