using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace FetchXmlWrapper
{
    [System.Serializable()]
    public class FetchWrapper
    {
        private FetchExpression fetchExpression = new FetchExpression();
        private FetchEntity fetchEntity = new FetchEntity();
        private List<object> fetchEntityItems = new List<object>();
        /// <summary>
        /// Initializes a new instance of the <see cref="FetchWrapper"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public FetchWrapper(FetchExpression fetch)
        {
            this.fetchExpression = fetch;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FetchWrapper"/> class.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        public FetchWrapper(string entityName)
        {
            fetchEntity.name = entityName;
            fetchExpression.Items = new FetchEntity[] { fetchEntity };
            fetchExpression.mapping = FetchMapping.logical;
            this.AddFetchEntityItem(new AllAttributes());
        }
        /// <summary>
        /// Adds an item to the entity node.
        /// </summary>
        /// <param name="item">The item to add</param>
        protected void AddFetchEntityItem(object item)
        {
            this.fetchEntityItems.Add(item);
            fetchEntity.Items = this.fetchEntityItems.ToArray();
        }

        protected void RemoveFetchEntityItem(Type typeToRemove)
        {
            for (int counter = fetchEntityItems.Count - 1; counter >= 0; counter--)
            {
                if (fetchEntityItems[counter].GetType() == typeToRemove)
                {
                    fetchEntityItems.RemoveAt(counter);
                }
            }
            this.fetchEntity.Items = fetchEntityItems.ToArray();
        }

        /// <summary>
        /// Gets the query expression.
        /// </summary>
        /// <value>The query expression.</value>
        public FetchExpression FetchExpression
        {
            get
            {
                return fetchExpression;
            }
        }

        public void AddColumn(string columnName, Aggregate aggregate, string alias)
        {
            this.FetchExpression.aggregate = true;
            this.RemoveFetchEntityItem(typeof(AllAttributes));
            FetchAttribute att = new FetchAttribute();
            att.name = columnName;
            att.aggregate = aggregate;
            att.alias = alias;
            this.RemoveColumn(columnName);
            this.AddFetchEntityItem(att);
        }
        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public void AddColumn(string columnName)
        {
            this.RemoveFetchEntityItem(typeof(AllAttributes));
            FetchAttribute att = new FetchAttribute();
            att.name = columnName;
            this.RemoveColumn(columnName);
            this.AddFetchEntityItem(att);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public void RemoveColumn(string columnName)
        {
            for (int i = this.fetchEntityItems.Count - 1; i >= 0; i--)
            {
                if (this.fetchEntityItems[i] is FetchAttribute && ((FetchAttribute)this.fetchEntityItems[i]).name == columnName)
                {
                    this.fetchEntityItems.RemoveAt(i);
                }
            }

            this.fetchEntity.Items = this.fetchEntityItems.ToArray();
        }


        /// <summary>
        /// Clears the columns.
        /// </summary>
        public void ClearColumns()
        {
            this.RemoveFetchEntityItem(typeof(AllAttributes));
            this.RemoveFetchEntityItem(typeof(FetchAttribute));
        }
        /// <summary>
        /// true if there are no shared locks are issued against the data that would prohibit other transactions from modifying the data in the records returned from the query; otherwise, false.
        /// </summary>
        public bool NoLock
        {
            get
            {
                return this.fetchExpression.nolock;
            }
            set
            {
                this.fetchExpression.nolock = value;
            }
        }
        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public string[] Columns
        {
            get
            {
                ArrayList columns = new ArrayList();
                if (this.fetchEntityItems.Where(t => t.GetType() == typeof(AllAttributes)).Count() > 0)
                {
                    //TODO: Return all of the attributes from the metadata
                }
                else
                {
                    foreach (object item in this.fetchEntityItems)
                    {
                        FetchAttribute att = item as FetchAttribute;
                        if (att != null)
                        {
                            columns.Add(att.name);
                        }
                    }
                }
                return (string[])columns.ToArray(typeof(string));
            }

            set
            {
                foreach (string col in value)
                {
                    this.AddColumn(col);
                }
            }
        }
        /// <summary>
        /// Converts to fetch XML.
        /// </summary>
        /// <returns></returns>
        public string ConvertToFetchXml()
        {
            return this.fetchExpression.Serialize();
        }

        /// <summary>
        /// Appends the order.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="orderType">Type of the order.</param>
        public void AppendOrder(string attributeName, bool descending)
        {
            FetchOrder order = new FetchOrder();
            order.attribute = attributeName;
            order.descending = descending;
            this.AddFetchEntityItem(order);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, Guid attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, string attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }
        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, int attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }
        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, double attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, float attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, decimal attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        public void AppendFilter(string attributeName, bool attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, Guid attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, string attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, int attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, double attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, float attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, decimal attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, bool attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, Guid attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, string attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, int attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, double attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, float attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, decimal attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, bool attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Internals the append filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionExpression">The condition expression.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        protected void InternalAppendFilter(string attributeName, object attributeValue, FetchCondition conditionExpression, FetchFilterType logicalOperator)
        {
            FetchFilter filter = new FetchFilter();
            filter.type = logicalOperator;
            List<object> items = new List<object>();
            items.Add(conditionExpression);
            items.AddRange(fetchEntity.Items.Where(f => f.GetType() == typeof(FetchFilter)).ToList());
            filter.Items = items.ToArray();
            this.RemoveFetchEntityItem(typeof(FetchFilter));
            this.AddFetchEntityItem(filter);
        }



        /// <summary>
        /// Appends the link.
        /// </summary>
        /// <param name="link">The link.</param>
        public void AppendLink(FetchLinkWrapper link)
        {
            this.AddFetchEntityItem(link.LinkEntity);
        }


        #region Public Methods


        #endregion
    }

    [System.Serializable()]
    public class FetchLinkWrapper
    {
        private FetchLinkEntity linkEntity;
        private List<object> linkEntityItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchLinkWrapper"/> class.
        /// </summary>
        /// <param name="fromEntityName">Name of from entity.</param>
        /// <param name="fromAttributeName">Name of from attribute.</param>
        /// <param name="toEntityName">Name of to entity.</param>
        /// <param name="toAttributeName">Name of to attribute.</param>
        public FetchLinkWrapper(string fromAttributeName, string toEntityName, string toAttributeName, LinkType linkType)
            : this(fromAttributeName, toEntityName, toAttributeName)
        {
            linkEntity = new FetchLinkEntity();
            linkEntityItems = new List<object>();
            linkEntity.name = toEntityName;
            linkEntity.from = fromAttributeName;
            linkEntity.to = toAttributeName;
            linkEntity.linktype = linkType.ToString();
        }

        public FetchLinkWrapper(string fromAttributeName, string toEntityName, string toAttributeName)
            : this(fromAttributeName, toEntityName, toAttributeName, LinkType.inner)
        {
        }


        public void AddColumn(string columnName, Aggregate aggregate, string alias)
        {
            this.RemoveLinkEntityItem(typeof(AllAttributes));
            FetchAttribute att = new FetchAttribute();
            att.name = columnName;
            att.aggregate = aggregate;
            att.alias = alias;
            this.RemoveColumn(columnName);
            this.AddLinkEntityItem(att);
        }
        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public void AddColumn(string columnName)
        {
            this.RemoveLinkEntityItem(typeof(AllAttributes));
            FetchAttribute att = new FetchAttribute();
            att.name = columnName;
            this.RemoveColumn(columnName);
            this.AddLinkEntityItem(att);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public void RemoveColumn(string columnName)
        {
            for (int i = this.linkEntityItems.Count - 1; i >= 0; i--)
            {
                if (this.linkEntityItems[i] is FetchAttribute && ((FetchAttribute)this.linkEntityItems[i]).name == columnName)
                {
                    this.linkEntityItems.RemoveAt(i);
                }
            }

            this.LinkEntity.Items = this.linkEntityItems.ToArray();
        }


        /// <summary>
        /// Clears the columns.
        /// </summary>
        public void ClearColumns()
        {
            this.RemoveLinkEntityItem(typeof(AllAttributes));
            this.RemoveLinkEntityItem(typeof(FetchAttribute));
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public string[] Columns
        {
            get
            {
                ArrayList columns = new ArrayList();
                if (this.linkEntityItems.Where(t => t.GetType() == typeof(AllAttributes)).Count() > 0)
                {
                    //TODO: Return all of the attributes from the metadata
                }
                else
                {
                    foreach (object item in this.linkEntityItems)
                    {
                        FetchAttribute att = item as FetchAttribute;
                        if (att != null)
                        {
                            columns.Add(att.name);
                        }
                    }
                }
                return (string[])columns.ToArray(typeof(string));
            }

            set
            {
                foreach (string col in value)
                {
                    this.AddColumn(col);
                }
            }
        }

        public string EntityAlias
        {
            get
            {
                return this.LinkEntity.alias;
            }
            set
            {
                this.LinkEntity.alias = value;
            }
        }
        /// <summary>
        /// Gets the link entity.
        /// </summary>
        /// <value>The link entity.</value>
        public FetchLinkEntity LinkEntity
        {
            get
            {
                return linkEntity;
            }
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, Guid attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, string attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }
        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, int attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }
        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, double attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, float attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AppendFilter(string attributeName, decimal attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        public void AppendFilter(string attributeName, bool attributeValue)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), FetchFilterType.and);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, Guid attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, string attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, int attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, double attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, float attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, decimal attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, bool attributeValue, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, Guid attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, string attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, int attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, double attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, float attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, decimal attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Appends the filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        public void AppendFilter(string attributeName, bool attributeValue, FetchOperator fetchOperator, FetchFilterType logicalOperator)
        {
            this.InternalAppendFilter(attributeName, attributeValue, FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, fetchOperator), logicalOperator);
        }

        /// <summary>
        /// Internals the append filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionExpression">The condition expression.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        protected void InternalAppendFilter(string attributeName, object attributeValue, FetchCondition conditionExpression, FetchFilterType logicalOperator)
        {
            FetchFilter filter = new FetchFilter();
            filter.type = logicalOperator;
            List<object> items = new List<object>();
            items.Add(conditionExpression);
            items.AddRange(linkEntity.Items.Where(f => f.GetType() == typeof(FetchFilter)).ToList());
            filter.Items = items.ToArray();
            this.RemoveLinkEntityItem(typeof(FetchFilter));
            this.AddLinkEntityItem(filter);
        }

        /// <summary>
        /// Appends the link.
        /// </summary>
        /// <param name="link">The link.</param>
        public void AppendLink(FetchLinkWrapper link)
        {
            this.AddLinkEntityItem(link.LinkEntity);
        }

        /// <summary>
        /// Adds an item to the entity node.
        /// </summary>
        /// <param name="item">The item to add</param>
        protected void AddLinkEntityItem(object item)
        {
            this.linkEntityItems.Add(item);
            this.LinkEntity.Items = this.linkEntityItems.ToArray();
        }

        protected void RemoveLinkEntityItem(Type typeToRemove)
        {
            for (int counter = linkEntityItems.Count - 1; counter >= 0; counter--)
            {
                if (linkEntityItems[counter].GetType() == typeToRemove)
                {
                    linkEntityItems.RemoveAt(counter);
                }
            }
            this.LinkEntity.Items = linkEntityItems.ToArray();
        }

    }

    [System.Serializable()]
    public class FetchWrapperHelper
    {

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, Guid attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, string attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, int attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, float attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, double attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, decimal attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, bool attributeValue)
        {
            return FetchWrapperHelper.CreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Internals the create condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        protected static FetchCondition InternalCreateFetchCondition(string attributeName, object attributeValue)
        {
            return FetchWrapperHelper.InternalCreateFetchCondition(attributeName, attributeValue, FetchOperator.eq);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, Guid attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, string attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, int attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, double attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, float attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, decimal attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Creates the condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">if set to <c>true</c> [attribute value].</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        public static FetchCondition CreateFetchCondition(string attributeName, bool attributeValue, FetchOperator fetchOperator)
        {
            return InternalCreateFetchCondition(attributeName, attributeValue, fetchOperator);
        }

        /// <summary>
        /// Internals the create condition expression.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <returns></returns>
        protected static FetchCondition InternalCreateFetchCondition(string attributeName, object attributeValue, FetchOperator fetchOperator)
        {
            FetchCondition conditionExpression = new FetchCondition();
            conditionExpression.attribute = attributeName;
            conditionExpression.@operator = fetchOperator;
            if (attributeValue != null)
            {
                conditionExpression.value = attributeValue.ToString();
            }

            return conditionExpression;
        }
    }
}
