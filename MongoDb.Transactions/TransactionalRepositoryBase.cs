using MongoDb.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MongoDb.Transactions
{
    public abstract class TransactionalRepositoryBase<T> : EntityRepositoryBase<T> where T : IEntity
    {
        internal delegate T AddEntityHandler(T entity);
        internal delegate void RemoveEntityHandler(T entity);
        internal delegate T UpdateEntityHandler(T entity);

        internal AddEntityHandler NonTxAdd;
        internal RemoveEntityHandler NonTxRemove;
        internal UpdateEntityHandler NonTxUpdate;

        public TransactionalRepositoryBase(string collection) : this(collection, null, null)
        {
        }

        public TransactionalRepositoryBase(string collection, string connectionString, string database) : base(collection, connectionString, database)
        {
            NonTxAdd = new AddEntityHandler(base.Add);
            NonTxRemove = new RemoveEntityHandler(base.Remove);
            NonTxUpdate = new UpdateEntityHandler(base.Update);
        }

        public override T Add(T entity)
        {
            if (Transaction.Current != null)
            {
                TransactionalEntity<T> txEntity = new TransactionalEntity<T>(default(T), entity, this, EntityRepositoryCommandsEnum.Add);
                MongoResourceManager<T> txRm = new MongoResourceManager<T>(txEntity);

                Transaction.Current.EnlistVolatile(txRm, EnlistmentOptions.None);

                Console.WriteLine(String.Format("Enlist manager for TX : {0} - Command : {1}", Transaction.Current.TransactionInformation.LocalIdentifier, EntityRepositoryCommandsEnum.Add.ToString()));

                return txEntity.Add();
            }
            else 
            {
                Console.WriteLine(String.Format("Perform TXless Command : {0}", EntityRepositoryCommandsEnum.Add.ToString()));

                return NonTxAdd(entity);
            }
        }

        public override void Remove(T entity)
        {
            if (Transaction.Current != null)
            {
                TransactionalEntity<T> txEntity = new TransactionalEntity<T>(entity, default(T), this, EntityRepositoryCommandsEnum.Remove);
                MongoResourceManager<T> txRm = new MongoResourceManager<T>(txEntity);

                Transaction.Current.EnlistVolatile(txRm, EnlistmentOptions.None);

                Console.WriteLine(String.Format("Enlist manager for TX : {0} - Command : {1}", Transaction.Current.TransactionInformation.LocalIdentifier, EntityRepositoryCommandsEnum.Remove.ToString()));

                txEntity.Remove();
            }
            else
            {
                Console.WriteLine(String.Format("Perform TXless Command : {0}", EntityRepositoryCommandsEnum.Remove.ToString()));

                NonTxRemove(entity);
            }
        }

        public override T Update(T entity)
        {
            if (Transaction.Current != null)
            {
                T original = this.Get(entity.Id);
                TransactionalEntity<T> txEntity = new TransactionalEntity<T>(original, entity, this, EntityRepositoryCommandsEnum.Remove);
                MongoResourceManager<T> txRm = new MongoResourceManager<T>(txEntity);

                Transaction.Current.EnlistVolatile(txRm, EnlistmentOptions.None);

                Console.WriteLine(String.Format("Enlist manager for TX : {0} - Command : {1}", Transaction.Current.TransactionInformation.LocalIdentifier, EntityRepositoryCommandsEnum.Update.ToString()));

                return txEntity.Update();
            }
            else
            {
                Console.WriteLine(String.Format("Perform TXless Command : {0}", EntityRepositoryCommandsEnum.Update.ToString()));

                return NonTxUpdate(entity);
            }
        }
    }
}
