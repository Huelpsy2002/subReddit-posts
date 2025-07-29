import React from "react";
import { FaArrowUp, FaArrowDown, FaRegComment, FaUser,FaHeart } from "react-icons/fa";



function Post(props) {

    function saveFavPost(e){
        e.preventDefault(); // Prevent default behavior
        e.stopPropagation(); // Stop event bubbling
        e.currentTarget.classList.toggle("active")

    }

    return (
    <a href={props.post.postUrl} target="_blank" style={{ textDecoration: "none", color: "inherit" }}>
        <div className="post">

            <div className="votes">
                <FaArrowUp />
                <p>{props.post.votes}</p>
                <FaHeart className="faviorite" onClick={saveFavPost} />
            </div>
          
            <h4>{props.post.title}</h4>
            <div className="author-comments">
                <p><FaUser /> {props.post.author}</p>

                <p> <FaRegComment /> {props.post.comments}</p>
                
                
            </div>
            <p>{props.post.content}</p>
            

        </div>
    </a>
    );
}


export default Post