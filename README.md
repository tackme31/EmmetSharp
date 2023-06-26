# EmmetSharp
An Emmet abbreviation parser written in C#.

**This software is in early stage of development. The APIs will be likely to change.**

## Usage
To expand an abbreviation, use the `Emment.Expand` method.
```csharp
using EmmetSharp;

var result = Emmet.Expand("div>p{Hello, EmmetSharp!}");
// => "<div><p>Hello, EmmetSharp!</p></div>"
```

If you want to format the HTML tags before rendering, call the method with the `tagFormatter` argument.

```csharp
using EmmetSharp;

var result = Emmet.Expand("div>p{Hello, EmmetSharp!}", tag => {
    tag.Text = tag.Text.ToUpper();
    return tag;
});
// => "<div><p>HELLO, EMMETSHARP!</p></div>"
```

## Implemented Features
### Syntax
- [x] Child (`div>p`)
- [x] Sibling (`p+p`)
- [x] Climb-up (`p>em^bq`)
- [x] Multiplication (`li*5`)
- [x] Grouping (`p+(div>h1)+p>a`)
- [x] ID & Class (`div#id`, `a.class1.class2`)
- [x] Custom attributes (`input[type="checkbox" checked]`)
- [x] Item numbering (`ul>li.item$*5`)
    - [x] Changing direction (`ul>li.item$@-*5`)
    - [x] Changing base (`ul>li.item$@3*5`)
- [x] Text (`a{Content}`)
    - [x] Without tag (`{Click }+a{here}`)

### Other Features
- [ ] Indentation
- [ ] Implicit tag names
- [ ] "Lorem Ipsum" generator 

## See also
- [Emmet &#8212; the essential toolkit for web-developers](https://emmet.io/)

## License
*EmmetSharp* is licensed under the MIT license. See LICENSE.txt.

## Author
- Takumi Yamada (xirtardauq@gmail.com)

---
