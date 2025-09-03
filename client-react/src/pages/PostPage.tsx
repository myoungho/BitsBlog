import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import type { Post } from "../hooks/usePosts";

export function PostPage() {
  const { id } = useParams<{ id: string }>();
  const [post, setPost] = useState<Post | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const res = await fetch(`${import.meta.env.VITE_API_URL}/api/posts/${id}`);
        if (!res.ok) throw new Error(`오류 상태: ${res.status}`);
        const data = (await res.json()) as Post;
        setPost(data);
      } catch (e: any) {
        setError(e?.message ?? "오류가 발생했습니다.");
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [id]);

  if (loading) return <div>불러오는 중...</div>;
  if (error || !post)
    return (
      <div>
        <p style={{ color: "red" }}>오류: {error ?? "알 수 없는 오류"}</p>
        <Link to="/">목록</Link>
      </div>
    );

  return (
    <div>
      <div style={{ marginBottom: 8 }}>
        <Link to="/">목록</Link>
        <Link to={`/edit/${post.id}`} style={{ marginLeft: 8 }}>
          수정
        </Link>
      </div>
      <h1>{post.title}</h1>
      <div style={{ color: "#666", marginBottom: 12 }}>
        작성일: {new Date(post.created).toLocaleString()}
      </div>
      <p>{post.content}</p>
    </div>
  );
}

