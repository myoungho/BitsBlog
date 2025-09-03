import { Link } from "react-router-dom";
import { usePosts } from "../hooks/usePosts";
import { PostsList } from "../components/PostsList";

export function ListPage() {
  const { posts, reload } = usePosts();

  async function handleDelete(id: number) {
    if (!confirm("삭제하시겠습니까?")) return;
    await fetch(`${import.meta.env.VITE_API_URL}/api/posts/${id}`, {
      method: "DELETE",
    });
    reload();
  }

  return (
    <div>
      <h1>게시글</h1>
      <div style={{ margin: "8px 0 16px" }}>
        <Link to="/new">글쓰기</Link>
      </div>
      <PostsList posts={posts} onDelete={handleDelete} />
    </div>
  );
}
