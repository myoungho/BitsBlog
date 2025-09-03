import { useState } from "react";
import type { Post } from "../hooks/usePosts";
import { Button, Form } from "react-bootstrap";
import { CKEditor } from '@ckeditor/ckeditor5-react';
// @ts-ignore - classic build has no types package
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

interface CreatePostProps {
  onCreated?: (post: Post) => void;
}

export function CreatePost({ onCreated }: CreatePostProps) {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (!title.trim() || !content.trim()) {
      setError("제목과 내용을 입력해 주세요.");
      return;
    }
    setSubmitting(true);
    try {
      const base = import.meta.env.VITE_API_URL?.replace(/\/$/, "") ?? "";
      const res = await fetch(`${base}/api/posts`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title, content }),
      });
      if (!res.ok) {
        throw new Error(`요청 실패: ${res.status}`);
      }
      const created = (await res.json()) as Post;
      onCreated?.(created);
      setTitle("");
      setContent("");
    } catch (err: any) {
      setError(err?.message ?? "등록 중 오류가 발생했습니다.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      {error && (
        <div className="text-danger mb-2">오류: {error}</div>
      )}
      <Form.Group className="mb-3" controlId="title">
        <Form.Label>제목</Form.Label>
        <Form.Control
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          disabled={submitting}
        />
      </Form.Group>
      <Form.Group className="mb-3" controlId="content">
        <Form.Label>내용</Form.Label>
        <div className="border rounded">
          <CKEditor
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            editor={ClassicEditor as any}
            data={content}
            onChange={(_, editor) => setContent(editor.getData())}
            config={{ language: 'ko' }}
          />
        </div>
      </Form.Group>
      <Button type="submit" disabled={submitting}>
        {submitting ? "저장 중..." : "등록"}
      </Button>
    </form>
  );
}
