using System;
using System.Text;
using System.Security.Cryptography;

namespace ProScanAlert
{
	/// <summary>
	/// Class for the Encryption.
	/// </summary>
	/// <description>
	/// This class is used to encrypt/decrypt the password to log into a PSM server and take control of it
	/// </description>
	public class Encryption
	{
		//! @cond PRIVATE
		private const string TDES_KEY = "30a5SS3356430643uSEDB243";
		private const string TDES_IV = "524GD763";
		//! @endcond

		/// <summary>
		/// Encrypt the specified string p.
		/// </summary>
		/// <param name="p">String to encrypt </param>
		/// <seealso cref="Decrypt"/>
		/// <returns>Base64 encrypted string </returns>
		public string Encrypt(string p)
		{
			if (p.Trim ().Length == 0)
				return string.Empty;

			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider ();
			ICryptoTransform cTransform;

			byte[] toEncArray = UTF8Encoding.UTF8.GetBytes (p);
			byte[] keyArray = UTF8Encoding.UTF8.GetBytes (TDES_KEY);
			byte[] ivArray = UTF8Encoding.UTF8.GetBytes (TDES_IV);

			cTransform = tdes.CreateEncryptor (keyArray, ivArray);

			byte[] resultArray = cTransform.TransformFinalBlock (toEncArray, 0, toEncArray.Length);

			tdes.Clear ();

			return Convert.ToBase64String (resultArray, 0, resultArray.Length);
		}

		/// <summary>
		/// Decrypt the specified string p.
		/// </summary>
		/// <param name="p">String to decrypt </param>
		/// <seealso cref="Encrypt"/>
		/// <returns>UTF8 encoded decrypted string </returns>
		public string Decrypt(string p)
		{
			if (p.Trim ().Length == 0)
				return string.Empty;

			byte[] toEncArray = Convert.FromBase64String (p);

			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider ();
			ICryptoTransform cTransform;

			byte[] keyArray = UTF8Encoding.UTF8.GetBytes (TDES_KEY);
			byte[] ivArray = UTF8Encoding.UTF8.GetBytes (TDES_IV);

			cTransform = tdes.CreateDecryptor (keyArray, ivArray);

			byte[] resultArray = cTransform.TransformFinalBlock (toEncArray, 0, toEncArray.Length);

			tdes.Clear ();

			return UTF8Encoding.UTF8.GetString(resultArray);
		}
	}
}

