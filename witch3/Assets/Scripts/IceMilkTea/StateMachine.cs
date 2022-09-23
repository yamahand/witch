// zlib/libpng License
//
// Copyright (c) 2018 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
#if ENABLE_IL2CPP
using UnityEngine.Scripting;
#endif

namespace IceMilkTea.Core
{
    /// <summary>
    /// �X�e�[�g�}�V���̍X�V�������ɔ��������A�������̗�O���ǂ��U�镑������\�������񋓌^�ł�
    /// </summary>
    public enum ImtStateMachineUnhandledExceptionMode
    {
        /// <summary>
        /// Update�֐����Ŕ���������O�����̂܂ܗ�O�Ƃ��Ĕ��������܂��B
        /// </summary>
        ThrowException,

        /// <summary>
        /// OnUnhandledException �n���h���ɓ]������܂��B
        /// </summary>
        CatchException,

        /// <summary>
        /// ���ݓ��쒆�X�e�[�g�� Error() �ɗ�O���]������܂��B
        /// �������A���ݓ��쒆�X�e�[�g�����݂��Ȃ��ꍇ�� ThrowException �Ɠ����̐U�镑���ɂȂ�܂��B
        /// </summary>
        CatchStateException,
    }



    #region �W���X�e�[�g�}�V��������
    /// <summary>
    /// �R���e�L�X�g�������Ƃ̂ł���X�e�[�g�}�V���N���X�ł�
    /// </summary>
    /// <typeparam name="TContext">���̃X�e�[�g�}�V�������R���e�L�X�g�̌^</typeparam>
    /// <typeparam name="TEvent">�X�e�[�g�}�V���֑��M����C�x���g�̌^</typeparam>
    public class ImtStateMachine<TContext, TEvent>
    {
        #region �X�e�[�g�N���X�{�̂Ɠ��ʃX�e�[�g�N���X�̒�`
        /// <summary>
        /// �X�e�[�g�}�V�������������Ԃ�\������X�e�[�g�N���X�ł��B
        /// </summary>
        public abstract class State
        {
            // �����o�ϐ���`
            internal Dictionary<TEvent, State> transitionTable;
            internal ImtStateMachine<TContext, TEvent> stateMachine;



            /// <summary>
            /// ���̃X�e�[�g����������X�e�[�g�}�V��
            /// </summary>
            protected ImtStateMachine<TContext, TEvent> StateMachine => stateMachine;


            /// <summary>
            /// ���̃X�e�[�g����������X�e�[�g�}�V���������Ă���R���e�L�X�g
            /// </summary>
            protected TContext Context => stateMachine.Context;



            /// <summary>
            /// �X�e�[�g�ɓ˓������Ƃ��̏������s���܂�
            /// </summary>
            protected internal virtual void Enter()
            {
            }


            /// <summary>
            /// �X�e�[�g���X�V����Ƃ��̏������s���܂�
            /// </summary>
            protected internal virtual void Update()
            {
            }


            /// <summary>
            /// �X�e�[�g����E�o�����Ƃ��̏������s���܂�
            /// </summary>
            protected internal virtual void Exit()
            {
            }


            /// <summary>
            /// �X�e�[�g�}�V���̖�������O�������������̏������s���܂��B
            /// ������ UnhandledExceptionMode �� CatchStateException �ł���K�v������܂��B
            /// </summary>
            /// <remarks>
            /// �����A���̊֐��� false ��Ԃ����ꍇ�́A��O�����ǖ�������ԂƔ��f����X�e�[�g�}�V����
            /// Update() �֐�����O�𑗏o���邱�ƂɂȂ�܂��B
            /// </remarks>
            /// <param name="exception">���������������̗�O</param>
            /// <returns>��O�����������ꍇ�� true ���A�������̏ꍇ�� false ��Ԃ��܂�</returns>
            protected internal virtual bool Error(Exception exception)
            {
                // �ʏ�͖������Ƃ��ĕԂ�
                return false;
            }


            /// <summary>
            /// �X�e�[�g�}�V�����C�x���g���󂯂鎞�ɁA���̃X�e�[�g�����̃C�x���g���K�[�h���܂�
            /// </summary>
            /// <param name="eventId">�n���ꂽ�C�x���gID</param>
            /// <returns>�C�x���g�̎�t���K�[�h����ꍇ�� true ���A�K�[�h�����C�x���g���󂯕t����ꍇ�� false ��Ԃ��܂�</returns>
            protected internal virtual bool GuardEvent(TEvent eventId)
            {
                // �ʏ�̓K�[�h���Ȃ�
                return false;
            }


            /// <summary>
            /// �X�e�[�g�}�V�����X�^�b�N�����X�e�[�g���|�b�v����O�ɁA���̃X�e�[�g�����̃|�b�v���K�[�h���܂�
            /// </summary>
            /// <returns>�|�b�v�̓�����K�[�h����ꍇ�� true ���A�K�[�h�����Ƀ|�b�v����𑱂���ꍇ�� false ��Ԃ��܂�</returns>
            protected internal virtual bool GuardPop()
            {
                // �ʏ�̓K�[�h���Ȃ�
                return false;
            }
        }



        /// <summary>
        /// �X�e�[�g�}�V���� "�C��" ��\��������ʂȃX�e�[�g�N���X�ł�
        /// </summary>
#if ENABLE_IL2CPP
        [Preserve]
#endif
        public sealed class AnyState : State { }
        #endregion



        #region �񋓌^��`
        /// <summary>
        /// �X�e�[�g�}�V����Update��Ԃ�\�����܂�
        /// </summary>
        private enum UpdateState
        {
            /// <summary>
            /// �A�C�h�����O���ł��B�܂艽�����Ă��܂���
            /// </summary>
            Idle,

            /// <summary>
            /// �X�e�[�g�̓˓��������ł�
            /// </summary>
            Enter,

            /// <summary>
            /// �X�e�[�g�̍X�V�������ł�
            /// </summary>
            Update,

            /// <summary>
            /// �X�e�[�g�̒E�o�������ł�
            /// </summary>
            Exit,
        }
        #endregion



        // �����o�ϐ���`
        private UpdateState updateState;
        private List<State> stateList;
        private State currentState;
        private State nextState;
        private Stack<State> stateStack;
        private HashSet<Func<Type, State>> stateFactorySet;



        /// <summary>
        /// �X�e�[�g�}�V���� Update() ���ɖ������̗�O�������������̃C�x���g�n���h���ł��B
        /// ������ UnhandledExceptionMode �v���p�e�B�� CatchException ���ݒ肳��Ă���K�v������܂��B
        /// false ���Ԃ����ƁA��O���������Ɣ��f���� Update() �֐�����O�𑗏o���܂��B
        /// </summary>
        public event Func<Exception, bool> UnhandledException;



        /// <summary>
        /// �X�e�[�g�}�V�����ێ����Ă���R���e�L�X�g
        /// </summary>
        public TContext Context { get; private set; }


        /// <summary>
        /// �X�e�[�g�}�V�����N�����Ă��邩�ǂ���
        /// </summary>
        public bool Running => currentState != null;


        /// <summary>
        /// �X�e�[�g�}�V�����A�X�V���������ǂ����B
        /// Update �֐����甲�����Ǝv���Ă��A���̃v���p�e�B�� true �������ꍇ�A
        /// Update ���ɗ�O�Ȃǂŕs���ȏI���̎d�������Ă���ꍇ���l�����܂��B
        /// </summary>
        public bool Updating => (Running && updateState != UpdateState.Idle);


        /// <summary>
        /// ���݂̃X�^�b�N���Ă���X�e�[�g�̐�
        /// </summary>
        public int StackCount => stateStack.Count;


        /// <summary>
        /// ���݂̃X�e�[�g�̖��O���擾���܂��B
        /// �܂��X�e�[�g�}�V�����N�����Ă��Ȃ��ꍇ�͋󕶎���ɂȂ�܂��B
        /// </summary>
        public string CurrentStateName => (Running ? currentState.GetType().Name : string.Empty);


        /// <summary>
        /// SendEvent() �֐��ɂ���Ĉ�x�A�J�ڏ�ԂɂȂ�����ɍĂ� SendEvent() �ɂ��J�ڂ������������邩�ǂ���
        /// </summary>
        public bool AllowRetransition { get; set; }


        /// <summary>
        /// �������̗�O�����������ۂ̐U�镑���̐ݒ�擾�����܂�
        /// </summary>
        public ImtStateMachineUnhandledExceptionMode UnhandledExceptionMode { get; set; }


        /// <summary>
        /// ���̃X�e�[�g�}�V�����Ō��Update�����X���b�hID
        /// </summary>
        public int LastUpdateThreadId { get; private set; }


        /// <summary>
        /// ���̃X�e�[�g�}�V�����Ō�Ɏ󂯕t�����C�x���gID
        /// </summary>
        public TEvent LastAcceptedEventID { get; private set; }



        /// <summary>
        /// ImtStateMachine �̃C���X�^���X�����������܂�
        /// </summary>
        /// <param name="context">���̃X�e�[�g�}�V�������R���e�L�X�g</param>
        /// <exception cref="ArgumentNullException">context �� null �ł�</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�N���X�̃C���X�^���X�̐����Ɏ��s���܂���</exception>
        public ImtStateMachine(TContext context)
        {
            // �n���ꂽ�R���e�L�X�g��null�Ȃ�
            if (context == null)
            {
                // null�͋�����Ȃ�
                throw new ArgumentNullException(nameof(context));
            }


            // �����o�̏�����������
            Context = context;
            stateList = new List<State>();
            stateStack = new Stack<State>();
            updateState = UpdateState.Idle;
            AllowRetransition = false;
            UnhandledExceptionMode = ImtStateMachineUnhandledExceptionMode.ThrowException;
            stateFactorySet = new HashSet<Func<Type, State>>();
        }


        #region �ėp���W�b�N�n
        /// <summary>
        /// �^����X�e�[�g�C���X�^���X�𐶐�����t�@�N�g���֐���o�^���܂�
        /// </summary>
        /// <param name="stateFactory">�o�^����t�@�N�g���֐�</param>
        /// <exception cref="ArgumentNullException">stateFactory �� null �ł�</exception>
        public void RegisterStateFactory(Func<Type, State> stateFactory)
        {
            // �n�b�V���Z�b�g�ɓo�^����
            stateFactorySet.Add(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }


        /// <summary>
        /// �o�^�����t�@�N�g���֐��̉��������܂�
        /// </summary>
        /// <param name="stateFactory">��������t�@�N�g���֐�</param>
        /// <exception cref="ArgumentNullException">stateFactory �� null �ł�</exception>
        public void UnregisterStateFactory(Func<Type, State> stateFactory)
        {
            // �n�b�V���Z�b�g����o�^����������
            stateFactorySet.Remove(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }
        #endregion


        #region �X�e�[�g�J�ڃe�[�u���\�z�n
        /// <summary>
        /// �X�e�[�g�̔C�ӑJ�ڍ\����ǉ����܂��B
        /// </summary>
        /// <remarks>
        /// ���̊֐��́A�J�ڌ����C�ӂ̏�Ԃ���̑J�ڂ���]����ꍇ�ɗ��p���Ă��������B
        /// �C�ӂ̑J�ڂ́A�ʏ�̑J�ځiAny�ȊO�̑J�ڌ��j���D��x���Ⴂ���Ƃɂ��A���ӂ����Ă��������B
        /// �܂��A�X�e�[�g�̑J�ڃe�[�u���ݒ�̓X�e�[�g�}�V�����N������O�Ɋ������Ȃ���΂Ȃ�܂���B
        /// </remarks>
        /// <typeparam name="TNextState">�C�ӏ�Ԃ���J�ڂ����ɂȂ�X�e�[�g�̌^</typeparam>
        /// <param name="eventId">�J�ڂ�������ƂȂ�C�x���gID</param>
        /// <exception cref="ArgumentException">���ɓ��� eventId ���ݒ肳�ꂽ�J�ڐ�X�e�[�g�����݂��܂�</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A���ɋN�����ł�</exception>
        public void AddAnyTransition<TNextState>(TEvent eventId) where TNextState : State, new()
        {
            // �P���ɑJ�ڌ���AnyState�Ȃ����̒P���ȑJ�ڒǉ��֐����Ă�
            AddTransition<AnyState, TNextState>(eventId);
        }


        /// <summary>
        /// �X�e�[�g�̑J�ڍ\����ǉ����܂��B
        /// �܂��A�X�e�[�g�̑J�ڃe�[�u���ݒ�̓X�e�[�g�}�V�����N������O�Ɋ������Ȃ���΂Ȃ�܂���B
        /// </summary>
        /// <typeparam name="TPrevState">�J�ڂ��錳�ɂȂ�X�e�[�g�̌^</typeparam>
        /// <typeparam name="TNextState">�J�ڂ����ɂȂ�X�e�[�g�̌^</typeparam>
        /// <param name="eventId">�J�ڂ�������ƂȂ�C�x���gID</param>
        /// <exception cref="ArgumentException">���ɓ��� eventId ���ݒ肳�ꂽ�J�ڐ�X�e�[�g�����݂��܂�</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A���ɋN�����ł�</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�N���X�̃C���X�^���X�̐����Ɏ��s���܂���</exception>
        public void AddTransition<TPrevState, TNextState>(TEvent eventId) where TPrevState : State, new() where TNextState : State, new()
        {
            // �X�e�[�g�}�V�����N�����Ă��܂��Ă���ꍇ��
            if (Running)
            {
                // �����ݒ�ł��Ȃ��̂ŗ�O��f��
                throw new InvalidOperationException("�X�e�[�g�}�V���́A���ɋN�����ł�");
            }


            // �J�ڌ��ƑJ�ڐ�̃X�e�[�g�C���X�^���X���擾
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();


            // �J�ڌ��X�e�[�g�̑J�ڃe�[�u���Ɋ��ɓ����C�x���gID�����݂��Ă�����
            if (prevState.transitionTable.ContainsKey(eventId))
            {
                // �㏑���o�^�������Ȃ��̂ŗ�O��f��
                throw new ArgumentException($"�X�e�[�g'{prevState.GetType().Name}'�ɂ́A���ɃC�x���gID'{eventId}'�̑J�ڂ��ݒ�ς݂ł�");
            }


            // �J�ڃe�[�u���ɑJ�ڂ�ݒ肷��
            prevState.transitionTable[eventId] = nextState;
        }


        /// <summary>
        /// �X�e�[�g�}�V�����N�����鎞�ɁA�ŏ��ɊJ�n����X�e�[�g��ݒ肵�܂��B
        /// </summary>
        /// <typeparam name="TStartState">�X�e�[�g�}�V�����N�����ɊJ�n����X�e�[�g�̌^</typeparam>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A���ɋN�����ł�</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�N���X�̃C���X�^���X�̐����Ɏ��s���܂���</exception>
        public void SetStartState<TStartState>() where TStartState : State, new()
        {
            // ���ɃX�e�[�g�}�V�����N�����Ă��܂��Ă���ꍇ��
            if (Running)
            {
                // �N�����Ă��܂����炱�̊֐��̑���͋�����Ȃ�
                throw new InvalidOperationException("�X�e�[�g�}�V���́A���ɋN�����ł�");
            }


            // ���ɏ�������X�e�[�g�̐ݒ������
            nextState = GetOrCreateState<TStartState>();
        }
        #endregion


        #region �X�e�[�g�X�^�b�N����n
        /// <summary>
        /// ���ݎ��s���̃X�e�[�g���A�X�e�[�g�X�^�b�N�Ƀv�b�V�����܂�
        /// </summary>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        public void PushState()
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ��O�𓊂���
            IfNotRunningThrowException();


            // ���݂̃X�e�[�g���X�^�b�N�ɐς�
            stateStack.Push(currentState);
        }


        /// <summary>
        /// �X�e�[�g�X�^�b�N�ɐς܂�Ă���X�e�[�g�����o���A�J�ڂ̏������s���܂��B
        /// </summary>
        /// <remarks>
        /// ���̊֐��̋����́A�C�x���gID�𑗂邱�Ƃ̂Ȃ��_�������� SendEvent �֐��Ɣ��Ɏ��Ă��܂��B
        /// ���� SendEvent �ɂ���Ď��̑J�ڂ̏������ł��Ă���ꍇ�́A�X�^�b�N����X�e�[�g�̓|�b�v����邱�Ƃ͂���܂���B
        /// </remarks>
        /// <returns>�X�^�b�N����X�e�[�g���|�b�v���ꎟ�̑J�ڂ̏��������������ꍇ�� true ���A�|�b�v����X�e�[�g���Ȃ�������A�X�e�[�g�ɂ��|�b�v���K�[�h���ꂽ�ꍇ�� false ��Ԃ��܂�</returns>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        public virtual bool PopState()
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ��O�𓊂���
            IfNotRunningThrowException();


            // ���������X�^�b�N����ł��邩�A���ɑJ�ڂ���X�e�[�g������ ���� �đJ�ڂ��������A�|�b�v����O�Ɍ��݂̃X�e�[�g�ɃK�[�h���ꂽ�̂Ȃ�
            if (stateStack.Count == 0 || (nextState != null && !AllowRetransition) || currentState.GuardPop())
            {
                // �|�b�v���̏o���Ȃ��̂�false��Ԃ�
                return false;
            }


            // �X�e�[�g���X�^�b�N������o���Ď��̃X�e�[�g�֑J�ڂ���悤�ɂ��Đ�����Ԃ�
            nextState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// �X�e�[�g�X�^�b�N�ɐς܂�Ă���X�e�[�g�����o���A���݂̃X�e�[�g�Ƃ��Ē����ɒ��ڐݒ肵�܂��B
        /// </summary>
        /// <remarks>
        /// ���̊֐��̋����� PopState() �֐��ƈႢ�A�|�b�v���ꂽ�X�e�[�g�����̂܂܌��ݏ������̃X�e�[�g�Ƃ��Ē����ɐݒ肷�邽�߁A
        /// ��Ԃ̑J�ڏ����͍s��ꂸ�A�|�b�v���ꂽ�X�e�[�g�� Enter() �͌Ăяo���ꂸ���̂܂܎��񂩂� Update() ���Ăяo�����悤�ɂȂ�܂��B
        /// </remarks>
        /// <returns>�X�^�b�N����X�e�[�g���|�b�v����A���݂̃X�e�[�g�Ƃ��Đݒ�o�����ꍇ�� true ���A�|�b�v����X�e�[�g���������A�|�b�v���K�[�h���ꂽ�ꍇ�� false ��Ԃ��܂�</returns>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        public virtual bool PopAndDirectSetState()
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ��O�𓊂���
            IfNotRunningThrowException();


            // ���������X�^�b�N����ł��邩�A�|�b�v����O�Ɍ��݂̃X�e�[�g�ɃK�[�h���ꂽ�̂Ȃ�
            if (stateStack.Count == 0 || currentState.GuardPop())
            {
                // �|�b�v���̏o���Ȃ��̂�false��Ԃ�
                return false;
            }


            // �X�e�[�g���X�^�b�N������o���Č��݂̃X�e�[�g�Ƃ��Đݒ肵�Đ�����Ԃ�
            currentState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// �X�e�[�g�X�^�b�N�ɐς܂�Ă���X�e�[�g������o���A���̂܂܎̂Ă܂��B
        /// </summary>
        /// <remarks>
        /// �X�e�[�g�X�^�b�N�̈�ԏ�ɐς܂�Ă���X�e�[�g�����̂܂܎̂Ă������ɗ��p���܂��B
        /// </remarks>
        public void PopAndDropState()
        {
            // �X�^�b�N����Ȃ�
            if (stateStack.Count == 0)
            {
                // ���������I��
                return;
            }


            // �X�^�b�N����X�e�[�g�����o���ĉ����������̂܂܎̂Ă�
            stateStack.Pop();
        }


        /// <summary>
        /// �X�e�[�g�X�^�b�N�ɐς܂�Ă��邷�ׂẴX�e�[�g���̂Ă܂��B
        /// </summary>
        public void ClearStack()
        {
            // �X�^�b�N����ɂ���
            stateStack.Clear();
        }
        #endregion


        #region �X�e�[�g�}�V������n
        /// <summary>
        /// ���ݎ��s���̃X�e�[�g���A�w�肳�ꂽ�X�e�[�g���ǂ����𒲂ׂ܂��B
        /// </summary>
        /// <typeparam name="TState">�m�F����X�e�[�g�̌^</typeparam>
        /// <returns>�w�肳�ꂽ�X�e�[�g�̏�Ԃł���� true ���A�قȂ�ꍇ�� false ��Ԃ��܂�</returns>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        public bool IsCurrentState<TState>() where TState : State
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ��O�𓊂���
            IfNotRunningThrowException();


            // ���݂̃X�e�[�g�ƌ^����v���邩�̏������̌��ʂ����̂܂ܕԂ�
            return currentState.GetType() == typeof(TState);
        }


        /// <summary>
        /// �X�e�[�g�}�V���ɃC�x���g�𑗐M���āA�X�e�[�g�J�ڂ̏������s���܂��B
        /// </summary>
        /// <remarks>
        /// �X�e�[�g�̑J�ڂ͒����ɍs��ꂸ�A���� Update �����s���ꂽ���ɑJ�ڏ������s���܂��B
        /// �܂��A���̊֐��ɂ��C�x���g��t�D�揇�ʂ́A��ԍŏ��ɑJ�ڂ��󂯓��ꂽ�C�x���g�݂̂ł��� Update �ɂ���đJ�ڂ����܂ŁA�㑱�̃C�x���g�͂��ׂĎ��s���܂��B
        /// ������ AllowRetransition �v���p�e�B�� true ���ݒ肳��Ă���ꍇ�́A�đJ�ڂ�������܂��B
        /// ����ɁA�C�x���g�̓X�e�[�g�� Enter �܂��� Update �������ł��󂯕t���邱�Ƃ��\�ŁA�X�e�[�g�}�V���� Update ����
        /// ���x���J�ڂ����邱�Ƃ��\�ł��� Exit ���ŃC�x���g�𑗂�ƁA�J�ڒ��ɂȂ邽�ߗ�O�����o����܂��B
        /// </remarks>
        /// <param name="eventId">�X�e�[�g�}�V���ɑ��M����C�x���gID</param>
        /// <returns>�X�e�[�g�}�V�������M���ꂽ�C�x���g���󂯕t�����ꍇ�� true ���A�C�x���g�����ۂ܂��́A�C�x���g�̎�t���ł��Ȃ��ꍇ�� false ��Ԃ��܂�</returns>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        /// <exception cref="InvalidOperationException">�X�e�[�g�� Exit �������̂��߃C�x���g���󂯕t���邱�Ƃ��o���܂���</exception>
        public virtual bool SendEvent(TEvent eventId)
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ��O�𓊂���
            IfNotRunningThrowException();


            // ���� Exit �������Ȃ�
            if (updateState == UpdateState.Exit)
            {
                // Exit ���� SendEvent �͋�����Ȃ�
                throw new InvalidOperationException("�X�e�[�g�� Exit �������̂��߃C�x���g���󂯕t���邱�Ƃ��o���܂���");
            }


            // ���ɑJ�ڏ��������Ă��� ���� �đJ�ڂ�������Ă��Ȃ��Ȃ�
            if (nextState != null && !AllowRetransition)
            {
                // �C�x���g�̎�t���o���Ȃ��������Ƃ�Ԃ�
                return false;
            }


            // ���݂̃X�e�[�g�ɃC�x���g�K�[�h���Ăяo���āA�K�[�h���ꂽ��
            if (currentState.GuardEvent(eventId))
            {
                // �K�[�h����Ď��s�������Ƃ�Ԃ�
                return false;
            }


            // ���ɑJ�ڂ���X�e�[�g�����݂̃X�e�[�g������o�����������Ȃ�������
            if (!currentState.transitionTable.TryGetValue(eventId, out nextState))
            {
                // �C�ӃX�e�[�g���炷����J�ڂ��o���Ȃ������̂Ȃ�
                if (!GetOrCreateState<AnyState>().transitionTable.TryGetValue(eventId, out nextState))
                {
                    // �C�x���g�̎�t���o���Ȃ�����
                    return false;
                }
            }


            // �Ō�Ɏ󂯕t�����C�x���gID���o���ăC�x���g�̎�t����������Ԃ�
            LastAcceptedEventID = eventId;
            return true;
        }


        /// <summary>
        /// �X�e�[�g�}�V���̏�Ԃ��X�V���܂��B
        /// </summary>
        /// <remarks>
        /// �X�e�[�g�}�V���̌��ݏ������Ă���X�e�[�g�̍X�V���s���܂����A�܂����N���̏ꍇ�� SetStartState �֐��ɂ���Đݒ肳�ꂽ�X�e�[�g���N�����܂��B
        /// �܂��A�X�e�[�g�}�V��������N�����̏ꍇ�A�X�e�[�g��Update�͌Ăяo���ꂸ�A���̍X�V���������s����鎞�ɂȂ�܂��B
        /// </remarks>
        /// <exception cref="InvalidOperationException">���݂̃X�e�[�g�}�V���́A�ʂ̃X���b�h�ɂ���čX�V���������s���Ă��܂��B[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]</exception>
        /// <exception cref="InvalidOperationException">���݂̃X�e�[�g�}�V���́A���ɍX�V���������s���Ă��܂�</exception>
        /// <exception cref="InvalidOperationException">�J�n�X�e�[�g���ݒ肳��Ă��Ȃ����߁A�X�e�[�g�}�V���̋N�����o���܂���</exception>
        public virtual void Update()
        {
            // �����X�e�[�g�}�V���̍X�V��Ԃ��A�C�h�����O�ȊO��������
            if (updateState != UpdateState.Idle)
            {
                // �����ʃX���b�h�����Update�ɂ�鑽�dUpdate�Ȃ�
                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (LastUpdateThreadId != currentThreadId)
                {
                    // �ʃX���b�h����̑��dUpdate�ł��邱�Ƃ��O�œf��
                    throw new InvalidOperationException($"���݂̃X�e�[�g�}�V���́A�ʂ̃X���b�h�ɂ���čX�V���������s���Ă��܂��B[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]");
                }


                // ���d��Update���Ăяo���Ȃ���O��f��
                throw new InvalidOperationException("���݂̃X�e�[�g�}�V���́A���ɍX�V���������s���Ă��܂�");
            }


            // Update�̋N���X���b�hID���o����
            LastUpdateThreadId = Thread.CurrentThread.ManagedThreadId;


            // �܂����N���Ȃ�
            if (!Running)
            {
                // ���ɏ�������ׂ��X�e�[�g�i�܂�N���J�n�X�e�[�g�j�����ݒ�Ȃ�
                if (nextState == null)
                {
                    // �N�����o���Ȃ���O��f��
                    throw new InvalidOperationException("�J�n�X�e�[�g���ݒ肳��Ă��Ȃ����߁A�X�e�[�g�}�V���̋N�����o���܂���");
                }


                // ���ݏ������X�e�[�g�Ƃ��Đݒ肷��
                currentState = nextState;
                nextState = null;


                try
                {
                    // Enter�������ł��邱�Ƃ�ݒ肵��Enter���Ă�
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }
                catch (Exception exception)
                {
                    // �N�����̕��A�͌��݂̃X�e�[�g��null�������Ă��Ȃ��Ƃ܂����̂őJ�ڑO�̏�Ԃɖ߂�
                    nextState = currentState;
                    currentState = null;


                    // �X�V��Ԃ��A�C�h�����O�ɂ��āA��O�������̃G���[�n���h�����O���s���I������
                    updateState = UpdateState.Idle;
                    DoHandleException(exception);
                    return;
                }


                // ���ɑJ�ڂ���X�e�[�g�������Ȃ�
                if (nextState == null)
                {
                    // �N�������͏I������̂ň�U�I���
                    updateState = UpdateState.Idle;
                    return;
                }
            }


            try
            {
                // ���ɑJ�ڂ���X�e�[�g�����݂��Ă��Ȃ��Ȃ�
                if (nextState == null)
                {
                    // Update�������ł��邱�Ƃ�ݒ肵��Update���Ă�
                    updateState = UpdateState.Update;
                    currentState.Update();
                }


                // ���ɑJ�ڂ���X�e�[�g�����݂��Ă���ԃ��[�v
                while (nextState != null)
                {
                    // Exit�������ł��邱�Ƃ�ݒ肵��Exit�������Ă�
                    updateState = UpdateState.Exit;
                    currentState.Exit();


                    // ���̃X�e�[�g�ɐ؂�ւ���
                    currentState = nextState;
                    nextState = null;


                    // Enter�������ł��邱�Ƃ�ݒ肵��Enter���Ă�
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }


                // �X�V�������I�������A�C�h�����O�ɖ߂�
                updateState = UpdateState.Idle;
            }
            catch (Exception exception)
            {
                // �X�V��Ԃ��A�C�h�����O�ɂ��āA��O�������̃G���[�n���h�����O���s���I������
                updateState = UpdateState.Idle;
                DoHandleException(exception);
                return;
            }
        }
        #endregion


        #region �������W�b�N�n
        /// <summary>
        /// ���������������̗�O���n���h�����O���܂�
        /// </summary>
        /// <param name="exception">���������������̗�O</param>
        /// <exception cref="ArgumentNullException">exception �� null �ł�</exception>
        private void DoHandleException(Exception exception)
        {
            // null��n���ꂽ��
            if (exception == null)
            {
                // �����n���h�����O����΂悢�̂�
                throw new ArgumentNullException(nameof(exception));
            }


            // �����A��O���E�����[�h ���� �n���h�����ݒ肳��Ă���Ȃ�
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchException && UnhandledException != null)
            {
                // �C�x���g���Ăяo���āA�������n���h�����O���ꂽ�̂Ȃ�
                if (UnhandledException(exception))
                {
                    // ���̂܂܏I��
                    return;
                }
            }


            // �����A��O���E���ăX�e�[�g�ɔC���郂�[�h ���� ���݂̎��s�X�e�[�g���ݒ肳��Ă���̂Ȃ�
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchStateException && currentState != null)
            {
                // �X�e�[�g�ɗ�O�𓊂��āA�������n���h�����O���ꂽ�̂Ȃ�
                if (currentState.Error(exception))
                {
                    // ���̂܂܏I��
                    return;
                }
            }


            // ��L�̃��[�h�ȊO�i�܂� ThrowException�j���A��O���n���h�����O����Ȃ������ifalse ��Ԃ��ꂽ�j�̂Ȃ��O���L���v�`�����Ĕ���������
            ExceptionDispatchInfo.Capture(exception).Throw();
        }


        /// <summary>
        /// �X�e�[�g�}�V�������N���̏ꍇ�ɗ�O�𑗏o���܂�
        /// </summary>
        /// <exception cref="InvalidOperationException">�X�e�[�g�}�V���́A�܂��N�����Ă��܂���</exception>
        protected void IfNotRunningThrowException()
        {
            // ���������܂����ݎ��s���̃X�e�[�g�����݂��Ă��Ȃ��Ȃ�
            if (!Running)
            {
                // �܂��N�����炵�Ă��Ȃ��̂ŗ�O��f��
                throw new InvalidOperationException("�X�e�[�g�}�V���́A�܂��N�����Ă��܂���");
            }
        }


        /// <summary>
        /// �w�肳�ꂽ�X�e�[�g�̌^�̃C���X�^���X���擾���܂����A���݂��Ȃ��ꍇ�͐������Ă���擾���܂��B
        /// �������ꂽ�C���X�^���X�́A���񂩂�擾�����悤�ɂȂ�܂��B
        /// </summary>
        /// <typeparam name="TState">�擾�A�܂��͐�������X�e�[�g�̌^</typeparam>
        /// <returns>�擾�A�܂��͐������ꂽ�X�e�[�g�̃C���X�^���X��Ԃ��܂�</returns>
        /// <exception cref="InvalidOperationException">�X�e�[�g�N���X�̃C���X�^���X�̐����Ɏ��s���܂���</exception>
        private TState GetOrCreateState<TState>() where TState : State, new()
        {
            // �X�e�[�g�̐������
            var stateType = typeof(TState);
            foreach (var state in stateList)
            {
                // �����Y���̃X�e�[�g�̌^�ƈ�v����C���X�^���X�Ȃ�
                if (state.GetType() == stateType)
                {
                    // ���̃C���X�^���X��Ԃ�
                    return (TState)state;
                }
            }


            // ���[�v���甲�����̂Ȃ�A�^��v����C���X�^���X�������Ƃ������Ȃ̂ŃC���X�^���X�𐶐����ăL���b�V������
            var newState = CreateStateInstanceCore<TState>() ?? throw new InvalidOperationException("�X�e�[�g�N���X�̃C���X�^���X�̐����Ɏ��s���܂���");
            stateList.Add(newState);


            // �V�����X�e�[�g�ɁA���g�̎Q�ƂƑJ�ڃe�[�u���̃C���X�^���X�̏��������s���ĕԂ�
            newState.stateMachine = this;
            newState.transitionTable = new Dictionary<TEvent, State>();
            return newState;
        }


        /// <summary>
        /// �w�肳�ꂽ�X�e�[�g�̌^�̃C���X�^���X�𐶐����܂��B
        /// </summary>
        /// <typeparam name="TState">��������ׂ��X�e�[�^�X�̌^</typeparam>
        /// <returns>���������C���X�^���X��Ԃ��܂�</returns>
        private TState CreateStateInstanceCore<TState>() where TState : State, new()
        {
            // ���ʂ��󂯎��ϐ���錾
            TState result;


            // �o�^����Ă���t�@�N�g���֐������
            var stateType = typeof(TState);
            foreach (var factory in stateFactorySet)
            {
                // ���������݂ăC���X�^���X���������ꂽ�̂Ȃ�
                result = (TState)factory(stateType);
                if (result != null)
                {
                    // ���̃C���X�^���X��Ԃ�
                    return result;
                }
            }


            // �t�@�N�g���֐��ł��ʖڂȂ�����������֐��ɗ���
            return CreateStateInstance<TState>();
        }


        /// <summary>
        /// �w�肳�ꂽ�X�e�[�g�̌^�̃C���X�^���X�𐶐����܂��B
        /// </summary>
        /// <typeparam name="TState">��������ׂ��X�e�[�^�X�̌^</typeparam>
        /// <returns>���������C���X�^���X��Ԃ��܂�</returns>
        protected virtual TState CreateStateInstance<TState>() where TState : State, new()
        {
            // ���蓮��̓W�F�l���b�N��new������݂̂ŕԂ�
            return new TState();
        }
        #endregion
    }
    #endregion



    #region ��int�C�x���g�^�x�[�X�̃X�e�[�g�}�V������
    /// <summary>
    /// �R���e�L�X�g�������Ƃ̂ł���X�e�[�g�}�V���N���X�ł�
    /// </summary>
    /// <typeparam name="TContext">���̃X�e�[�g�}�V�������R���e�L�X�g�̌^</typeparam>
    public class ImtStateMachine<TContext> : ImtStateMachine<TContext, int>
    {
        /// <summary>
        /// ImtStateMachine �̃C���X�^���X�����������܂�
        /// </summary>
        /// <param name="context">���̃X�e�[�g�}�V�������R���e�L�X�g</param>
        /// <exception cref="ArgumentNullException">context �� null �ł�</exception>
        public ImtStateMachine(TContext context) : base(context)
        {
        }
    }
    #endregion
}