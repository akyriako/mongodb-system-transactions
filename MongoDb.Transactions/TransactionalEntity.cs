using MongoDb.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MongoDb.Transactions
{
    public class TransactionalEntity<T> where T : IEntity
    {
        private T m_Original;
        public T Original
        {
          get { return m_Original; }
        }

        private T m_Current;
        public T Current
        {
            get { return m_Current; }
        }

        private TransactionalRepositoryBase<T> m_Repository;

        public TransactionalRepositoryBase<T> Repository
        {
            get { return m_Repository; }
        }

        private bool m_CommitWithSuccess = false;

        public bool CommitWithSuccess
        {
            get { return m_CommitWithSuccess; }
        }

        private bool m_RollbackWithSuccess = false;

        public bool RollbackWithSuccess
        {
            get { return m_RollbackWithSuccess; }
        }

        private bool m_Prepared = false;

        public bool Prepared
        {
            get { return m_Prepared; }
        }

        private EntityRepositoryCommandsEnum m_Command;

        public EntityRepositoryCommandsEnum Command
        {
            get { return m_Command; }
        }

        public TransactionalEntity(T original, T current, TransactionalRepositoryBase<T> repository, EntityRepositoryCommandsEnum command)
        {
            m_Original = original;
            m_Current = current;
            m_Repository = repository;
            m_Command = command;
        }

        public bool Commit()
        {
            Console.WriteLine(String.Format("Perform commit for TX : {0} - Command : {1}", "", m_Command.ToString()));
            // if it reached that far it means that all document are already in collection
            m_CommitWithSuccess = true;

            return m_CommitWithSuccess;
        }

        public bool Rollback()
        {
            Console.WriteLine(String.Format("Perform rollback for TX : {0} - Command : {1}", "", m_Command.ToString()));

            if (m_Command == EntityRepositoryCommandsEnum.Update)
            {
                m_Repository.NonTxUpdate(this.m_Original);
            }

            if (m_Command == EntityRepositoryCommandsEnum.Add)
            {
                m_Repository.NonTxRemove(this.m_Current);
            }

            if (m_Command == EntityRepositoryCommandsEnum.Remove)
            {
                m_Repository.NonTxAdd(this.m_Original);
            }

            m_RollbackWithSuccess = true;
            return m_RollbackWithSuccess;
        }

        public T Add()
        {
            T result = m_Repository.NonTxAdd(this.m_Current);
            m_Prepared = true;

            return result;
        }

        public void Remove()
        {
            m_Repository.NonTxRemove(this.Original);
            m_Prepared = true;
        }

        public T Update()
        {
            T result =  m_Repository.NonTxUpdate(this.m_Current);
            m_Prepared = true;

            return result;
        }
    }
}
