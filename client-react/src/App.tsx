import { PostsList } from "./components/PostsList";
import { usePosts } from "./hooks/usePosts";

export default function App() {
  const posts = usePosts();

  return (
    <div>
      <h1>게시판 (React + TS)</h1>
      <PostsList posts={posts} />
    </div>
  );
}
