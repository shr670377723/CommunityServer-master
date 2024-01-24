/*
 * CacheInfo.cs
 * 
 * Copyright � 2007 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;

namespace AjaxPro
{
	/// <summary>
	/// Represents a cache info object for HTTP caching using ETags and Last-Modified-Since headers.
	/// </summary>
	internal class CacheInfo
	{
		private string etag;
		private DateTime lastMod;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheInfo"/> class.
        /// </summary>
        /// <param name="etag">The etag.</param>
        /// <param name="lastMod">The last mod.</param>
		internal CacheInfo(string etag, DateTime lastMod)
		{
			this.etag = etag;
			this.lastMod = lastMod;
		}

        /// <summary>
        /// Gets the E tag.
        /// </summary>
        /// <value>The E tag.</value>
		internal string ETag
		{
			get{ return null; }
		}

        /// <summary>
        /// Gets the last modified.
        /// </summary>
        /// <value>The last modified.</value>
		internal DateTime LastModified
		{
			get{ return lastMod; }
		}
	}
}