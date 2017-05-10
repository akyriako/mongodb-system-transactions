using MongoDb.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MongoDb.Transactions
{
    public class MongoResourceManager<T> : IEnlistmentNotification where T : IEntity 
    {
        private TransactionalEntity<T> m_TxEntity;

        public MongoResourceManager(TransactionalEntity<T> txEntity)
        {
            m_TxEntity = txEntity;
        }

        public MongoResourceManager(T entity, TransactionalRepositoryBase<T> repository, EntityRepositoryCommandsEnum command)
        {
            T current = entity;
            T original = repository.Get(entity.Id);

            TransactionalEntity<T> txEntity = new TransactionalEntity<T>(original, current, repository, command);

            m_TxEntity = txEntity;
        }

        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine(String.Format("TX-RSM commits TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));

            bool success = this.m_TxEntity.Commit();

            if (success)
            {
                enlistment.Done();
                Console.WriteLine(String.Format("TX-RSM commited TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine(String.Format("TX-RSM is in doubt TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));

            Rollback(enlistment);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine(String.Format("TX-RSM is preparing TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));

            if (this.m_TxEntity.Prepared)
            {
                preparingEnlistment.Prepared();
                Console.WriteLine(String.Format("TX-RSM prepared TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine(String.Format("TX-RSM is rolling back TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));

            bool success = this.m_TxEntity.Rollback();

            if (success)
            {
                enlistment.Done();
                Console.WriteLine(String.Format("TX-RSM rolled back TX : {0} - Command : {1}", "", this.m_TxEntity.Command.ToString()));
            }
        }
    }
}
