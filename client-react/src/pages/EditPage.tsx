import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import type { Post } from "../hooks/usePosts";

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
        if (!res.ok) throw new Error(`요청 실패: ${res.status}`);
        const data = (await res.json()) as Post;
        if (!mounted) return;
        setTitle(data.title);
        setContent(data.content);
      } catch (e: any) {
        setError(e?.message ?? "게시글을 불러오는 중 오류가 발생했습니다.");
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
      if (!res.ok) throw new Error(`저장 실패: ${res.status}`);
      navigate("/");
    } catch (e: any) {
      setError(e?.message ?? "저장 중 오류가 발생했습니다.");
    }
  }

  if (loading) return <div>불러오는 중...</div>;
  if (error) return (
    <div>
      <p style={{ color: "red" }}>오류: {error}</p>
      <Link to="/">목록으로</Link>
    </div>
  );

  return (
    <div>
      <h1>게시글 수정</h1>
      <div style={{ marginBottom: 8 }}>
        <Link to="/">목록으로</Link>
      </div>
      <form onSubmit={onSubmit}>
        <div style={{ marginBottom: 8 }}>
          <label>
            제목
            <input value={title} onChange={(e) => setTitle(e.target.value)} />
          </label>
        </div>
        <div style={{ marginBottom: 8 }}>
          <label>
            내용
            <textarea
              rows={8}
              value={content}
              onChange={(e) => setContent(e.target.value)}
            />
          </label>
        </div>
        <button type="submit">저장</button>
      </form>
    </div>
  );
}
