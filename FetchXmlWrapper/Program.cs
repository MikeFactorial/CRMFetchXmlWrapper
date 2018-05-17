using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FetchXmlWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example: Static declaration of Fetch Xml
            string invoicePaymentSumFetch = string.Format(@" 
                <fetch distinct='false' mapping='logical' aggregate='true' no-lock='true'> 
                    <entity name='cobalt_invoicepayment'> 
                        <attribute name='cobalt_appliedamount' alias='cobalt_appliedamount_sum' aggregate='sum' /> 
                        <filter type='and'>
                <condition attribute='cobalt_paymentid' operator='eq' value='D4BDF2E8-BE5D-4B33-ABFF-96E33F968BD7' />    
                        </filter>
                    </entity> 
                </fetch>");

            //Example: Fetch Xml built using classes generated from the Fetch Xml Definition.
            FetchExpression fetch = new FetchExpression();
            fetch.distinct = false;
            fetch.mapping = FetchMapping.logical;
            fetch.aggregate = true;
            fetch.nolock = true;
            FetchEntity[] entity = new FetchEntity[] { new FetchEntity() };
            fetch.Items = entity;
            entity[0].name = "cobalt_invoicepayment";
            FetchAttribute attribute = new FetchAttribute();
            attribute.name = "cobalt_appliedamount";
            attribute.alias = "cobalt_appliedamount_sum";
            attribute.aggregate = Aggregate.sum;

            FetchFilter filter = new FetchFilter();
            filter.type = FetchFilterType.and;
            FetchCondition cond = new FetchCondition();
            cond.attribute = "cobalt_paymentid";
            cond.@operator = FetchOperator.eq;
            cond.value = "D4BDF2E8-BE5D-4B33-ABFF-96E33F968BD7";
            filter.Items = new FetchCondition[] { cond };

            entity[0].Items = new object[] { attribute, filter };

            string xml = fetch.Serialize();


            FetchWrapper fetchWrapper = new FetchWrapper("cobalt_invoicepayment");
            fetchWrapper.AppendFilter("cobalt_paymentid", new Guid("D4BDF2E8-BE5D-4B33-ABFF-96E33F968BD7"));
            fetchWrapper.AddColumn("cobalt_appliedamount", Aggregate.sum, "cobalt_appliedamount_sum");
            xml = fetchWrapper.FetchExpression.Serialize();

            FetchWrapper wrapperFromFetchXmlString = new FetchWrapper(FetchExpression.Deserialize(invoicePaymentSumFetch));
        }
    }
}
