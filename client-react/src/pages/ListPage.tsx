import { Link } from "react-router-dom";
import { usePosts } from "../hooks/usePosts";
import { PostsList } from "../components/PostsList";

export function ListPage() {
  const { posts } = usePosts();

  return (
    <div>
      <h1>게시글</h1>
      <div style={{ margin: "8px 0 16px" }}>
        <Link to="/new">글쓰기</Link>
      </div>
      <PostsList posts={posts} />
    </div>
  );
}

