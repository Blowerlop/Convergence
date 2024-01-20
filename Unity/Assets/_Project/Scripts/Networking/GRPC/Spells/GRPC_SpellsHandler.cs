using System;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Project.Spells;
using UnityEngine;

namespace Project
{
    public class GRPC_SpellsHandler : MonoBehaviour, IDisposable
    {        
        [SerializeField] private SOScriptableObjectReferencesCache scriptableObjectReferencesCache;
        
        private CancellationTokenSource _spellCastStreamCancelSrc;
        
        // Stream coming from GRPC. Used to receive spell cast requests from Unreal clients on Netcode server.
        private AsyncServerStreamingCall<GRPC_SpellCastRequest> _spellCastRequestStream;
        
        private void OnEnable()
        {
            GRPC_Transport.instance.onClientStopEvent.Subscribe(this, TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, StartSpellCastRequestStream);
            GRPC_NetworkManager.instance.onClientStoppedEvent.Subscribe(this, Dispose);
        }

        private void OnDisable()
        {
            if (GRPC_NetworkManager.isBeingDestroyed) return;
            
            GRPC_Transport.instance.onClientStopEvent.Unsubscribe(TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(StartSpellCastRequestStream);
            GRPC_NetworkManager.instance.onClientStoppedEvent.Unsubscribe(Dispose);
        }
        
        private void StartSpellCastRequestStream()
        {
            if (_spellCastRequestStream != null)
            {
                Debug.LogError(
                    "GRPC_SpellsHandler > Trying to start a SpellCastRequestStream when there is one already running.");
                return;
            }

            SpellCastRequestStream();
        }
        
        private async void SpellCastRequestStream()
        {
            _spellCastStreamCancelSrc = new CancellationTokenSource();
            _spellCastRequestStream = GRPC_Transport.instance.client.GRPC_SpellCastRequestGrpcToNetcode(new GRPC_EmptyMsg());

            try
            {
                while (await _spellCastRequestStream.ResponseStream.MoveNext(_spellCastStreamCancelSrc.Token))
                {
                    var request = _spellCastRequestStream.ResponseStream.Current;
                    HandleSpellCastRequest(request);
                }
            }
            catch (RpcException)
            { 
                if (GRPC_NetworkManager.instance.isConnected)
                    GRPC_NetworkManager.instance.StopClient();
            }
        }

        private bool IsRequestValid(GRPC_SpellCastRequest request, out SpellData spell)
        {
            spell = null;
            
            if (!UserInstanceManager.instance.TryGetUserInstance(request.ClientId, out var user))
            {
                Debug.LogError($"GRPC_SpellsHandler > Received a spell cast request (from GRPC) from client " +
                               $"with ID {request.ClientId}. But this client is not registered in UserInstanceManager.");
                return false;
            }

            if (!user.IsMobile)
            {
                Debug.LogError($"GRPC_SpellsHandler > Client {request.ClientId} is not a mobile client.");
                return false;
            }
            
            if (!SOCharacter.TryGetCharacter(scriptableObjectReferencesCache, user.CharacterId, out var character))
            {
                Debug.LogError($"GRPC_SpellsHandler > Client {request.ClientId} has an invalid character ID {user.CharacterId}.");
                return false;
            }
            
            if (!character.TryGetSpell(request.SpellIndex, out spell))
            {
                Debug.LogError($"GRPC_SpellsHandler > Client {request.ClientId} has an invalid spell index {request.SpellIndex}.");
                return false;
            }

            return true;
        }
        
        private void HandleSpellCastRequest(GRPC_SpellCastRequest request)
        {
            if (!IsRequestValid(request, out var spell)) return;
            
            ICastResult result = ICastResult.TypeToInstance(spell.requiredResultType);
            
            if (!result.TryFromCastRequest(request))
            {
                Debug.LogError(
                    $"GRPC_SpellsHandler > Client {request.ClientId} has invalid spell cast result params in his request.");
                return;
            }
            
            SpellManager.instance.TryCastSpell(request.ClientId, request.SpellIndex, result);
        }
        
        private void TokenCancel()
        {
            _spellCastStreamCancelSrc?.Cancel();
        }
        
        public void Dispose()
        {            
            _spellCastStreamCancelSrc?.Dispose();
            _spellCastRequestStream?.Dispose();

            _spellCastRequestStream = null;
        }
    }
}