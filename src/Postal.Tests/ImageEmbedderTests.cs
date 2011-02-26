﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace Postal
{
    public class ImageEmbedderTests
    {
        LinkedResource StubLinkedResource(string s) 
        { 
            return new LinkedResource(new MemoryStream()); 
        }

        [Fact]
        public void AddImage_returns_LinkedResource()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("test.png");
            resource.ShouldNotBeNull();
        }

        [Fact]
        public void Repeated_images_use_the_same_LinkedResource()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var r1 = embedder.AddImage("test-a.png");
            var r2 = embedder.AddImage("test-a.png");
            Assert.Same(r1, r2);
        }

        [Fact]
        public void Determine_content_type_from_PNG_file_extension()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("test.png");
            resource.ContentType.ShouldEqual(new ContentType("image/png"));
        }

        [Fact]
        public void Determine_content_type_from_PNG_http_file_extension()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("http://test.com/test.png");
            resource.ContentType.ShouldEqual(new ContentType("image/png"));
        }

        [Fact]
        public void Determine_content_type_from_JPEG_file_extension()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("test.jpeg");
            resource.ContentType.ShouldEqual(new ContentType("image/jpeg"));
        }

        [Fact]
        public void Determine_content_type_from_JPG_file_extension()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("test.jpg");
            resource.ContentType.ShouldEqual(new ContentType("image/jpeg"));
        }

        [Fact]
        public void Determine_content_type_from_GIF_file_extension()
        {
            var embedder = new ImageEmbedder(StubLinkedResource);
            var resource = embedder.AddImage("test.gif");
            resource.ContentType.ShouldEqual(new ContentType("image/gif"));
        }

        [Fact]
        public void Can_read_image_from_file_system()
        {
            var embedder = new ImageEmbedder();
            var filename = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(filename, new byte[] { 42 });
                using (var resource = embedder.AddImage(filename))
                {
                    resource.ContentStream.Length.ShouldEqual(1);
                }
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Fact]
        public void Can_read_image_from_http_url()
        {
            var embedder = new ImageEmbedder();
            using (var resource = embedder.AddImage("http://upload.wikimedia.org/wikipedia/commons/6/63/Wikipedia-logo.png"))
            {
                resource.ContentStream.Length.ShouldNotEqual(0);
            }
        }

        [Fact]
        public void PutImagesIntoView_adds_linked_resources()
        {
            var embedder = new ImageEmbedder(s => new LinkedResource(new MemoryStream()));
            var cid = embedder.AddImage("test.png");
            using (var view = AlternateView.CreateAlternateViewFromString("<img src=\"cid:" + cid.ContentId + "\" />", new ContentType("text/html")))
            {
                embedder.PutImagesIntoView(view);

                view.LinkedResources.Count.ShouldEqual(1);
                view.LinkedResources[0].ShouldBeSameAs(cid);
            }
        }
    }
}
