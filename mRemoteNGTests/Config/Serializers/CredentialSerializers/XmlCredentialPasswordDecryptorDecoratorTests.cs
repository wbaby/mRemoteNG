﻿using System.Linq;
using System.Security;
using mRemoteNG.Config.Serializers.CredentialSerializer;
using mRemoteNG.Security;
using mRemoteNG.Security.SymmetricEncryption;
using NSubstitute;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Serializers.CredentialSerializers
{
    public class XmlCredentialPasswordDecryptorDecoratorTests
    {
        private XmlCredentialPasswordDecryptorDecorator _sut;
        private IKeyProvider _keyProvider;
        private readonly SecureString _decryptionKey = "myKey1".ConvertToSecureString();
        private string _unencryptedPassword = "myPassword1";

        [SetUp]
        public void Setup()
        {
            var baseDeserializer = new XmlCredentialRecordDeserializer();
            _keyProvider = Substitute.For<IKeyProvider>();
            _sut = new XmlCredentialPasswordDecryptorDecorator(_keyProvider, baseDeserializer);
        }

        [Test]
        public void OutputedCredentialHasDecryptedPassword()
        {
            _keyProvider.GetKey().Returns(_decryptionKey);
            var xml = GenerateXml();
            var output = _sut.Deserialize(xml);
            Assert.That(output.First().Password.ConvertToUnsecureString(), Is.EqualTo(_unencryptedPassword));
        }


        private string GenerateXml()
        {
            var cryptoProvider = new AeadCryptographyProvider();
            var encryptedPassword = cryptoProvider.Encrypt(_unencryptedPassword, _decryptionKey);
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                $"<Credentials EncryptionEngine=\"{cryptoProvider.CipherEngine}\" BlockCipherMode=\"{cryptoProvider.CipherMode}\" KdfIterations=\"{cryptoProvider.KeyDerivationIterations}\" SchemaVersion=\"1.0\">" +
                    $"<Credential Id=\"ce6b0397-d476-4ffe-884b-dbe9347a88a8\" Title=\"New Credential\" Username=\"asdfasdf\" Domain=\"\" Password=\"{encryptedPassword}\" />" +
                "</Credentials>";
        }
    }
}