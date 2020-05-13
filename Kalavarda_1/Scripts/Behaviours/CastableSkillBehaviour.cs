using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

public class CastableSkillBehaviour : MonoBehaviour
{
    public Image ProgressBack;
    public Image ProgressTop;

    private RectTransform _progressBackRect, _progressTopRect;
    private readonly Player _player = Player.Instance;
    private ICastableSkill _currentCastableSkill;

    void Start()
    {
        _progressBackRect = ProgressBack.GetComponent<RectTransform>();
        _progressTopRect = ProgressTop.GetComponent<RectTransform>();

        _player.BeforeUseSkill += BeforePlayerUseSkill;
    }

    private void BeforePlayerUseSkill(ISkilled player, ISkill skill)
    {
        if (skill is ICastableSkill castableSkill)
        {
            _currentCastableSkill = castableSkill;
            castableSkill.OnEndCast += OnEndCastSkill;
        }
    }

    private void OnEndCastSkill(ICastableSkill skill)
    {
        skill.OnEndCast -= OnEndCastSkill;
        _currentCastableSkill = null;
    }

    void Update()
    {
        if (_currentCastableSkill != null)
        {
            ProgressBack.gameObject.SetActive(true);
            ProgressTop.gameObject.SetActive(true);

            _progressTopRect.sizeDelta = new Vector2(_progressBackRect.rect.width * _currentCastableSkill.CastedDurationNormalized, _progressBackRect.rect.height);
            _progressTopRect.offsetMin = _progressBackRect.offsetMin;
        }
        else
        {
            ProgressBack.gameObject.SetActive(false);
            ProgressTop.gameObject.SetActive(false);

            //_progressTopRect.sizeDelta = new Vector2(0, _progressBackRect.rect.height);
        }
    }
}
