﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Tests.Stubs;
using Xunit;

namespace MrCMS.Tests.Entities
{
    public class DocumentTests
    {
        [Fact]
        public void Document_CanDelete_IsTrueWhenNoChildren()
        {
            var doc = new StubDocument();

            doc.SetChildren(new List<Document>());

            doc.CanDelete.Should().BeTrue();
        }

        [Fact]
        public void Document_CanDelete_IsFalseWhenChildrenAreAdded()
        {
            var doc = new StubDocument();

            doc.SetChildren(new List<Document> {new StubDocument()});

            doc.CanDelete.Should().BeFalse();
        }

        [Fact]
        public void Document_PublishedChildren_DoesNotReturnNonWebpages()
        {
            var doc = new StubDocument();

            doc.SetChildren(new List<Document> {new StubDocument()});

            doc.PublishedChildren.Should().HaveCount(0);
        }

        [Fact]
        public void Document_PublishedChildren_PublishedWebpageIsReturned()
        {
            var doc = new StubDocument();

            var document = new TextPage {PublishOn = DateTime.UtcNow.AddDays(-1)};
            doc.SetChildren(new List<Document> {document});

            doc.PublishedChildren.Should().Contain(document);
        }

        [Fact]
        public void Document_PublishedChildren_UnpublishedWebpageIsNotReturned()
        {
            var doc = new StubDocument();

            var document = new TextPage {PublishOn = null};
            doc.SetChildren(new List<Document> {document});

            doc.PublishedChildren.Should().NotContain(document);
        }

        [Fact]
        public void Document_OnDeleting_RemovesDocumentFromParent()
        {
            var doc = new StubDocument();

            var child = new StubDocument();
            doc.SetChildren(new List<Document> { child });

            child.OnDeleting();

            doc.Children.Should().NotContain(child);
        }
    }
}