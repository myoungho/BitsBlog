import { Link } from "react-router-dom";
import { usePosts } from "../hooks/usePosts";
import { PostsList } from "../components/PostsList";
import { Button } from "react-bootstrap";

export function ListPage() {
  const { posts } = usePosts();

  return (
    <div>
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h1 className="h4 m-0">게시글 목록</h1>
        <Link to="/new" className="btn btn-primary">새 글 작성</Link>
      </div>
      <PostsList posts={posts} />
    </div>
  );
}
