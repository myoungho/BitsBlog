import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import type { Post } from "../hooks/usePosts";
import { Button, Card, Form } from "react-bootstrap";
import { CKEditor } from '@ckeditor/ckeditor5-react';
// @ts-ignore - classic build has no types package
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

export function EditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");

  useEffect(() => {
    let mounted = true;
    async function load() {
      try {
        const base = import.meta.env.VITE_API_URL?.replace(/\/$/, "") ?? "";
        const res = await fetch(`${base}/api/posts/${id}`);
        if (!res.ok) throw new Error(`Request failed: ${res.status}`);
        const data = (await res.json()) as Post;
        if (!mounted) return;
        setTitle(data.title);
        setContent(data.content);
      } catch (e: any) {
        setError(e?.message ?? "An error occurred while loading the post.");
      } finally {
        setLoading(false);
      }
    }
    load();
    return () => {
      mounted = false;
    };
  }, [id]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const base = import.meta.env.VITE_API_URL?.replace(/\/$/, "") ?? "";
      const res = await fetch(`${base}/api/posts/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title, content }),
      });
      if (!res.ok) throw new Error(`Save failed: ${res.status}`);
      navigate("/");
    } catch (e: any) {
      setError(e?.message ?? "An error occurred while saving.");
    }
  }

  if (loading) return <div>Loading...</div>;
  if (error) return (
    <div>
      <p className="text-danger">Error: {error}</p>
      <Link to="/" className="btn btn-outline-secondary">Back to List</Link>
    </div>
  );

  return (
    <div>
      <div className="mb-3">
        <Link to="/" className="btn btn-outline-secondary">Back to List</Link>
      </div>

      <Card>
        <Card.Body>
          <h1 className="h4 mb-3">Edit Post</h1>
          <Form onSubmit={onSubmit}>
            <Form.Group className="mb-3" controlId="title">
              <Form.Label>Title</Form.Label>
              <Form.Control value={title} onChange={(e) => setTitle(e.target.value)} />
            </Form.Group>
            <Form.Group className="mb-3" controlId="content">
              <Form.Label>Content</Form.Label>
              <div className="border rounded">
                <CKEditor
                  // eslint-disable-next-line @typescript-eslint/no-explicit-any
                  editor={ClassicEditor as any}
                  data={content}
                  onChange={(_, editor) => setContent(editor.getData())}
                  config={{ language: 'en' }}
                />
              </div>
            </Form.Group>
            <Button type="submit">저장</Button>
          </Form>
        </Card.Body>
      </Card>
    </div>
  );
}
