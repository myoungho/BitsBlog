import { useState } from "react";
import type { Post } from "../hooks/usePosts";

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
      setError("제목과 내용을 입력하세요.");
      return;
    }
    setSubmitting(true);
    try {
      const res = await fetch(`${import.meta.env.VITE_API_URL}/api/posts`, {
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
    <form onSubmit={handleSubmit} style={{ marginBottom: 16 }}>
      <h2>글쓰기</h2>
      {error && (
        <div style={{ color: "red", marginBottom: 8 }}>오류: {error}</div>
      )}
      <div style={{ marginBottom: 8 }}>
        <label>
          제목
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            disabled={submitting}
            style={{ display: "block", width: "100%" }}
          />
        </label>
      </div>
      <div style={{ marginBottom: 8 }}>
        <label>
          내용
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            disabled={submitting}
            rows={5}
            style={{ display: "block", width: "100%" }}
          />
        </label>
      </div>
      <button type="submit" disabled={submitting}>
        {submitting ? "등록 중..." : "등록"}
      </button>
    </form>
  );
}

